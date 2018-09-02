using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallLauncher : MonoBehaviour
{
    [SerializeField] Rigidbody m_ballRigidbody = null;
    [SerializeField] float m_launchSpeed = 0.2f;

    public bool OutOfPlay { get; private set; }

	void Awake ()
    {
        if (m_ballRigidbody == null)
        {
            m_ballRigidbody = GetComponent<Rigidbody>();
        }
        Launch();	
	}

    public void Launch()
    {
        var lauchVel = new Vector3((Random.value * 2.0f) - 1.0f, (Random.value * 2.0f) - 1.0f, 1.0f);
        lauchVel.Normalize();
        m_ballRigidbody.position = Vector3.zero;
        m_ballRigidbody.velocity = lauchVel * m_launchSpeed;
        OutOfPlay = false;
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "NearWall")
        {
            OutOfPlay = true;
        }
    }
}
