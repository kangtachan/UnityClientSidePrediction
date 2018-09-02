using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class DebugUI : MonoBehaviour {

    [SerializeField] MockServer m_mockServer = null;
    [SerializeField] Client m_client = null;
    [SerializeField] Text m_latencyText = null;
    [SerializeField] Slider m_latencySlider = null;
    [SerializeField] Text m_score = null;
    
	void Start ()
    {
        UpdateLatency();
	}
	
	public void UpdateLatency()
    {
        m_mockServer.Latency = Mathf.Lerp(0, 2.0f, m_latencySlider.value);
        m_latencyText.text = "Latency (" + (int)(m_mockServer.Latency * 1000) + "ms) ";
    }

    private void Update()
    {
        m_score.text = "Score " + m_client.Score;
    }
}
