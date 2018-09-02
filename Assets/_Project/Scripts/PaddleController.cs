using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaddleController : MonoBehaviour
{
    [SerializeField] float m_impulseStrength = 1.0f;

    private Rigidbody m_rigidbody;

    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody>();
    }
    
    private void FixedUpdate()
    {
        ApplyInput();
    }

    private void ApplyInput()
    {
        if (Input.GetKey(KeyCode.W))
        {
            m_rigidbody.AddForce(Vector3.up * m_impulseStrength, ForceMode.Impulse);
        }
        if (Input.GetKey(KeyCode.S))
        {
            m_rigidbody.AddForce(Vector3.down * m_impulseStrength, ForceMode.Impulse);
        }
        if (Input.GetKey(KeyCode.A))
        {
            m_rigidbody.AddForce(Vector3.left * m_impulseStrength, ForceMode.Impulse);
        }
        if (Input.GetKey(KeyCode.D))
        {
            m_rigidbody.AddForce(Vector3.right * m_impulseStrength, ForceMode.Impulse);
        }
    }
}
