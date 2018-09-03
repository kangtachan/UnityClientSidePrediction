using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ViewingClient : MonoBehaviour, IClient
{
    public event Action<InputMessage> NewClientMessage = null;

    [SerializeField] GameObject m_sceneRoot = null;
    [SerializeField] Transform m_paddle;

    private ServerStateMessage m_latestStateMessage;
    
    public void SetSceneActive(bool active)
    {
        m_sceneRoot.SetActive(active);
    }
    
    public void ReceiveServerMessage(ServerStateMessage stateMessage)
    {
        if (stateMessage.tick > m_latestStateMessage.tick)
        {
            m_latestStateMessage = stateMessage;

            // Whenever we get a server message, interpolate the client to this position
            // Thus, viewing client see's the paddle in the past
            // First element list is paddle. 
            m_paddle.transform.position = stateMessage.rigidbody_states[0].position;
        }
    }
}
