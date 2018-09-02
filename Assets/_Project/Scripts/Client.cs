using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Client : MonoBehaviour
{
    public event Action<InputMessage> NewClientMessage;
    
    [SerializeField] GameObject m_sceneRoot = null;
    [SerializeField] float PositionErrorThreshold = 0.0001f;
    [SerializeField] float RotationErrorThreshold = 0.001f;

    private const int BUFFER_SIZE = 1024;

    private Queue<ServerStateMessage> m_stateMessages = new Queue<ServerStateMessage>();
    private Inputs[] m_inputBuffer = new Inputs[BUFFER_SIZE];
    private ClientState[] m_stateBuffer = new ClientState[BUFFER_SIZE];
    private uint m_tick = 0;
    private Rigidbody[] m_syncedRigidbodies = null;
    private PaddleController m_paddle = null;

    private void Awake()
    {
        m_syncedRigidbodies = GetComponentsInChildren<Rigidbody>(true);
        m_paddle = GetComponentInChildren<PaddleController>(true);
    }

    public void SetSceneActive(bool active)
    {
        m_sceneRoot.SetActive(active);
    }

    public void ReceiveServerMessage(ServerStateMessage stateMessage)
    {
        m_stateMessages.Enqueue(stateMessage);
    }

    private void FixedUpdate()
    {
        uint bufferIndex = m_tick % BUFFER_SIZE;

        StoreCurrentState(bufferIndex);

        Inputs inputs = GetInputForFrame();

        ApplyInputClientSide(inputs);

        m_inputBuffer[bufferIndex] = inputs;

        Physics.Simulate(Time.fixedDeltaTime);
        
        SendInputToServer(inputs);

        ProcessServerMessages(Time.fixedDeltaTime);

        m_tick++;
    }

    private void StoreCurrentState(uint bufferIndex)
    {
        ClientState state;
        state.rigidbody_states = new RigidbodyStateNoVel[m_syncedRigidbodies.Length];
        for (int i = 0; i < m_syncedRigidbodies.Length; i++)
        {
            state.rigidbody_states[i].position = m_syncedRigidbodies[i].position;
            state.rigidbody_states[i].rotation = m_syncedRigidbodies[i].rotation;
        }
        m_stateBuffer[bufferIndex] = state;
    }

    private Inputs GetInputForFrame()
    {
        Inputs inputs;
        inputs.up = Input.GetKey(KeyCode.W);
        inputs.down = Input.GetKey(KeyCode.S);
        inputs.left = Input.GetKey(KeyCode.A);
        inputs.right = Input.GetKey(KeyCode.D);
        return inputs;
    }

    private void ApplyInputClientSide(Inputs inputs)
    {
        m_paddle.ApplyInput(inputs);
    }

    private void SendInputToServer(Inputs inputs)
    {
        InputMessage input_msg;
        input_msg.input = inputs;
        if (NewClientMessage != null)
        {
            NewClientMessage(input_msg);
        }
    }

    private void ProcessServerMessages(float dt)
    {
        while (m_stateMessages.Count > 0) 
        {
            ServerStateMessage message = m_stateMessages.Dequeue();
            if (DoesAnyObjectNeedCorrection(message))
            {
                // Set all the rigidbodies state to that of the received server message
                SetRigidbodyStates(message);

                // Re-apply all the inputs from the tick of that server message
                ResimulateFromTick(message.tick, dt);
            }
        }
    }

    private bool DoesAnyObjectNeedCorrection(ServerStateMessage serverStateMessage)
    {
        uint bufferIndex = serverStateMessage.tick % BUFFER_SIZE;

        for (int i = 0; i < serverStateMessage.rigidbody_states.Length; i++)
        {
            RigidbodyState serverState = serverStateMessage.rigidbody_states[i];
            RigidbodyStateNoVel clientState = m_stateBuffer[bufferIndex].rigidbody_states[i];

            Vector3 position_error = serverState.position - clientState.position;
            float rotation_error = 1.0f - Quaternion.Dot(serverState.rotation, clientState.rotation);

            if (position_error.sqrMagnitude > PositionErrorThreshold * PositionErrorThreshold 
                || rotation_error > RotationErrorThreshold * RotationErrorThreshold)
            {
                return true;
            }
        }
        return false;
    }

    private void SetRigidbodyStates(ServerStateMessage stateMessage)
    {
        for (int i = 0; i < stateMessage.rigidbody_states.Length; i++)
        {
            RigidbodyState serverRigidbodyState = stateMessage.rigidbody_states[i];
            Rigidbody clientRigidbody = m_syncedRigidbodies[i];
            clientRigidbody.position = serverRigidbodyState.position;
            clientRigidbody.rotation = serverRigidbodyState.rotation;
            clientRigidbody.velocity = serverRigidbodyState.velocity;
            clientRigidbody.angularVelocity = serverRigidbodyState.angular_velocity;
        }
    }
    
    private void ResimulateFromTick(uint startTick, float dt)
    {
        while (startTick <= m_tick)
        {
            uint bufferIndex = startTick % BUFFER_SIZE;

            StoreCurrentState(bufferIndex);

            ApplyInputClientSide(m_inputBuffer[bufferIndex]);

            Physics.Simulate(dt);

            ++startTick;
        }
    }
    
    private struct ClientState
    {
        public RigidbodyStateNoVel[] rigidbody_states;
    }

    public struct RigidbodyStateNoVel
    {
        public Vector3 position;
        public Quaternion rotation;
    }
}
