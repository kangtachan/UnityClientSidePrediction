using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Receive input from client, and simulates physics to determine the state.
/// Has authority over whether the ball has been missed, and decides the score.
/// Sends snapshot states to the clients.
/// Fake latency can be applied to the sent messages.
/// </summary>
public class MockServer : MonoBehaviour
{
    public event Action<ServerStateMessage> NewServerMessage;

    [SerializeField] GameObject[] m_clientRefs = null;
    [SerializeField] GameObject m_simulationRoot = null;
    [SerializeField] GameObject m_syncRoot = null;
    [SerializeField] float m_timeBetweenSnapshots = 0.1f; 

    private Queue<InputMessage> m_receivedMessages = new Queue<InputMessage>();
    private Queue<DelayedStateMessage> m_messagesToSend = new Queue<DelayedStateMessage>();
    private uint m_tick;
    private BallLauncher[] m_balls = null;
    private Rigidbody[] m_syncedRigidbodies = null;
    private PaddleController m_paddle = null;
    private float m_snapshotTimeAccum = 0;
    private IClient[] m_clients = null;

    public uint Score { get; private set; }
    public float Latency { get; set; }
    
    void Awake ()
    {
        m_clients = new IClient[m_clientRefs.Length];
        for (int i = 0; i < m_clientRefs.Length; i++)
        {
            m_clients[i] = m_clientRefs[i].GetComponent<IClient>();
            NewServerMessage += m_clients[i].ReceiveServerMessage;
            m_clients[i].NewClientMessage += ReceiveClientMessage;
        }
        
        m_balls = m_syncRoot.GetComponentsInChildren<BallLauncher>(true);
        m_syncedRigidbodies = m_syncRoot.GetComponentsInChildren<Rigidbody>(true);
        m_paddle = m_syncRoot.GetComponentInChildren<PaddleController>(true);
    }

    public void ReceiveClientMessage(InputMessage input_msg)
    {
        m_receivedMessages.Enqueue(input_msg);
    }

    void FixedUpdate()
    {
        // Disable the client, so Physics.Simulate does not process the client objects
        for (int i = 0; i < m_clients.Length; i++)
        {
            m_clients[i].SetSimulationActive(false);
        }
        m_simulationRoot.SetActive(true);

        UpdateServer(Time.fixedDeltaTime);
        
        CreateOutgoingMessages();

        DispatchOutgoingMessages(Time.fixedDeltaTime);

        // Turn client objects back on
        for (int i = 0; i < m_clients.Length; i++)
        {
            m_clients[i].SetSimulationActive(true);
        }
        m_simulationRoot.SetActive(false);

        m_tick++;
    }

    private void UpdateServer(float dt)
    {
        while (m_receivedMessages.Count > 0)
        {
            // Server decides if ball is out of play
            for (int i = 0; i < m_balls.Length; i++)
            {
                if (m_balls[i].OutOfPlay)
                {
                    m_balls[i].Launch();
                    Score++;
                }
            }

            InputMessage input_msg = m_receivedMessages.Dequeue();
            m_paddle.ApplyInput(input_msg.input);

            Physics.Simulate(dt);
        }
    }

    private void CreateOutgoingMessages()
    {
        // Create messages and add to queue
        // Not sending immediately to accomodate fake latency
        ServerStateMessage stateMessage;

        stateMessage.tick = m_tick;
        stateMessage.score = Score;

        stateMessage.rigidbody_states = new RigidbodyState[m_syncedRigidbodies.Length];
        for (int i_rb = 0; i_rb < m_syncedRigidbodies.Length; i_rb++)
        {
            stateMessage.rigidbody_states[i_rb].position = m_syncedRigidbodies[i_rb].position;
            stateMessage.rigidbody_states[i_rb].rotation = m_syncedRigidbodies[i_rb].rotation;
            stateMessage.rigidbody_states[i_rb].velocity = m_syncedRigidbodies[i_rb].velocity;
            stateMessage.rigidbody_states[i_rb].angular_velocity = m_syncedRigidbodies[i_rb].angularVelocity;
        }
        m_messagesToSend.Enqueue(new DelayedStateMessage()
        {
            sendTime = Time.time + Latency,
            message = stateMessage
        });
    }
    
    private void DispatchOutgoingMessages(float dt)
    {
        m_snapshotTimeAccum += dt;
        if (m_snapshotTimeAccum > m_timeBetweenSnapshots)
        {
            m_snapshotTimeAccum -= dt;

            // Send messages after fake latency time is reached
            while (m_messagesToSend.Count > 0 && m_messagesToSend.Peek().sendTime <= Time.time)
            {
                ServerStateMessage message = m_messagesToSend.Dequeue().message;

                if (NewServerMessage != null)
                {
                    NewServerMessage(message);
                }
            }
        }
    }

    struct DelayedStateMessage
    {
        public float sendTime;
        public ServerStateMessage message;
    }
}
