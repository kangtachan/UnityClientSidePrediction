using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothFollowTransform : MonoBehaviour
{
    [SerializeField] Transform m_target = null;
    [SerializeField] float m_lerpRate = 0.5f;

    private Transform m_transform;

    private void Awake()
    {
        m_transform = transform;
    }

    void Update ()
    {
        SmoothToTarget();	
	}
    
    private void SmoothToTarget()
    {
        //todo: replace with framerate independent smoothing
        m_transform.position = Vector3.Lerp(m_transform.position, m_target.position, m_lerpRate);
        m_transform.rotation = Quaternion.Slerp(m_transform.rotation, m_target.rotation, m_lerpRate);
    }
}
