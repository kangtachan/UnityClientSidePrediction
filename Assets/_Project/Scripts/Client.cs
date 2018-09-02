using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Client : MonoBehaviour
{
    public event Action<InputMessage> NewClientMessage;

    [SerializeField] Rigidbody[] m_syncedRigibodies = null;
    [SerializeField] GameObject m_sceneRoot = null;

    private Queue<StateMessage> m_stateMessages = new Queue<StateMessage>();

    public void SetSceneActive(bool active)
    {
        m_sceneRoot.SetActive(active);
    }

    public void ReceiveServerMessage(StateMessage stateMessage)
    {
        m_stateMessages.Enqueue(stateMessage);
    }

    private void SetRigidbodyStates(StateMessage stateMessage)
    {
        for (int i = 0; i < stateMessage.rigidbody_states.Length; i++)
        {
            RigidbodyState serverRigidbodyState = stateMessage.rigidbody_states[i];
            Rigidbody clientRigidbody = m_syncedRigibodies[i];
            clientRigidbody.position = serverRigidbodyState.position;
            clientRigidbody.rotation = serverRigidbodyState.rotation;
            clientRigidbody.velocity = serverRigidbodyState.velocity;
            clientRigidbody.angularVelocity = serverRigidbodyState.angular_velocity;
        }
    }

    private void FixedUpdate()
    {
        SendInputToServer(Time.fixedDeltaTime);

        ProcessServerMessages(Time.fixedDeltaTime);
    }

    private void SendInputToServer(float dt)
    {
        Inputs inputs;
        inputs.up = Input.GetKey(KeyCode.W);
        inputs.down = Input.GetKey(KeyCode.S);
        inputs.left = Input.GetKey(KeyCode.A);
        inputs.right = Input.GetKey(KeyCode.D);

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
            StateMessage message = m_stateMessages.Dequeue();
            SetRigidbodyStates(message);
            Physics.Simulate(dt);
        }
    }
}
