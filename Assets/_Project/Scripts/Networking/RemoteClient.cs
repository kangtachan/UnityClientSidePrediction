using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// On update from server, sets all objects directly to the server position. No client
/// side prediction.
/// Displayed objects tracked the received positions in m_syncObjects, using 
/// SmoothFollowTransform script. This is set up in the scene.
/// </summary>
public class RemoteClient : MonoBehaviour, IClient
{
    public event Action<InputMessage> NewClientMessage = null;

    [SerializeField] Transform m_syncRoot = null;

    private ServerStateMessage m_latestStateMessage;
    private List<Transform> m_syncObjects = new List<Transform>();

    private void Awake()
    {
        foreach (Transform tr in m_syncRoot)
        {
            m_syncObjects.Add(tr);
        }
    }

    public void SetSimulationActive(bool active)
    {
        // View client does no simulation
    }
    
    public void ReceiveServerMessage(ServerStateMessage stateMessage)
    {
        if (stateMessage.tick > m_latestStateMessage.tick)
        {
            m_latestStateMessage = stateMessage;

            // Whenever we get a server message, interpolate the client to this position
            // Thus, viewing client see's the paddle in the past
            SetRigidbodyStates(stateMessage);
        }
    }

    private void SetRigidbodyStates(ServerStateMessage stateMessage)
    {
        // Override the client rigidbodies with the server rigidbody parameters
        for (int i = 0; i < stateMessage.rigidbody_states.Length; i++)
        {
            RigidbodyState serverRigidbodyState = stateMessage.rigidbody_states[i];
            Transform clientRigidbody = m_syncObjects[i];
            clientRigidbody.position = serverRigidbodyState.position;
            clientRigidbody.rotation = serverRigidbodyState.rotation;
        }
    }
}
