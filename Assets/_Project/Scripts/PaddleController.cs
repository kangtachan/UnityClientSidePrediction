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
    
    public void ApplyInput(Inputs inputs)
    {
        if (inputs.up)
        {
            m_rigidbody.AddForce(Vector3.up * m_impulseStrength, ForceMode.Impulse);
        }
        if (inputs.down)
        {
            m_rigidbody.AddForce(Vector3.down * m_impulseStrength, ForceMode.Impulse);
        }
        if (inputs.left)
        {
            m_rigidbody.AddForce(Vector3.left * m_impulseStrength, ForceMode.Impulse);
        }
        if (inputs.right)
        {
            m_rigidbody.AddForce(Vector3.right * m_impulseStrength, ForceMode.Impulse);
        }
    }
}
