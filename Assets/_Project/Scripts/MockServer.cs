using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MockServer : MonoBehaviour
{
    public event Action<ServerStateMessage> NewServerMessage;

    [SerializeField] Client m_client = null;
    [SerializeField] GameObject m_sceneRoot = null;

    private Queue<InputMessage> m_receivedMessages = new Queue<InputMessage>();
    private Queue<DelayedStateMessage> m_messagesToSend = new Queue<DelayedStateMessage>();
    private uint m_tick;
    private BallLauncher[] m_balls = null;
    private Rigidbody[] m_syncedRigidbodies = null;
    private PaddleController m_paddle = null;

    public uint Score { get; private set; }
    public float Latency { get; set; }
    
    void Awake ()
    {
        m_client.NewClientMessage += ReceiveClientMessage;
        NewServerMessage += m_client.ReceiveServerMessage;
        m_balls = GetComponentsInChildren<BallLauncher>(true);
        m_syncedRigidbodies = GetComponentsInChildren<Rigidbody>(true);
        m_paddle = GetComponentInChildren<PaddleController>(true);
    }

    public void ReceiveClientMessage(InputMessage input_msg)
    {
        m_receivedMessages.Enqueue(input_msg);
    }

    void FixedUpdate ()
    {
        m_client.SetSceneActive(false);
        m_sceneRoot.SetActive(true);

        UpdateServer(Time.fixedDeltaTime);
        
        CreateOutgoingMessages();

        DispatchOutgoingMessages();

        m_client.SetSceneActive(true);
        m_sceneRoot.SetActive(false);

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
    
    private void DispatchOutgoingMessages()
    {
        while (m_messagesToSend.Count > 0 && m_messagesToSend.Peek().sendTime <= Time.time)
        {
            ServerStateMessage message = m_messagesToSend.Dequeue().message;

            if (NewServerMessage != null)
            {
                NewServerMessage(message);
            }
        }
    }

    struct DelayedStateMessage
    {
        public float sendTime;
        public ServerStateMessage message;
    }

}
