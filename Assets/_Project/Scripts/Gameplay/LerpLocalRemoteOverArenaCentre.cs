using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpLocalRemoteOverArenaCentre : MonoBehaviour
{
    [SerializeField] Transform m_localTarget = null;
    [SerializeField] Transform m_remoteTarget = null;
    [SerializeField] float m_interpolationDistance = 1.0f;
    /// <summary>
    /// Anchor at the centre of the area, rotated with forward vector pointing to opponent's side
    /// </summary>
    [SerializeField] Transform m_arenaCentreForward;
    
    private Transform m_transform;

    private void Awake()
    {
        m_transform = transform;
    }

    void Update()
    {
        SmoothToTarget();
    }

    private void SmoothToTarget()
    {
        Vector3 averageTargetPos = m_localTarget.position + m_remoteTarget.position;
        Vector3 centreToTarget = averageTargetPos - m_arenaCentreForward.position;
        float centreToTargetProjForward = Vector3.Dot(centreToTarget, m_arenaCentreForward.forward);
        float lerpLocalToRemote = m_interpolationDistance != 0
            ? Mathf.Clamp01(0.5f + (centreToTargetProjForward / m_interpolationDistance))
            : centreToTargetProjForward > 0 ? 1.0f : 0;

        m_transform.position = Vector3.Lerp(m_localTarget.position, m_remoteTarget.position, lerpLocalToRemote);
        m_transform.rotation = Quaternion.Slerp(m_localTarget.rotation, m_remoteTarget.rotation, lerpLocalToRemote);
    }
}
