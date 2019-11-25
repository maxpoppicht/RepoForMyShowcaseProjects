using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Tobias Stroedicke

public class CameraInvert : MonoBehaviour {

    /// <summary>
    /// Invert Gameobjects (0 = x, 1 = y)
    /// </summary>
    [SerializeField]
    private GameObject[] m_InvertObjects;

    /// <summary>
    /// Invert Text (0 = x, 1 = y)
    /// </summary>
    [SerializeField]
    private Text[] m_Texts;

#pragma warning disable 0649
    /// <summary>
    /// Invert Checker (0 = x, 1 = y)
    /// </summary>
    [SerializeField]
    private Toggle[] m_Toggles;
#pragma warning restore
#pragma warning disable 0649
    [SerializeField]
    private Cinemachine.CinemachineFreeLook m_FreeLook;
#pragma warning restore

    /// <summary>
    /// Set Mouse X Axis Speed
    /// </summary>
    public void SetMouseInvert(int _index)
    {
        if (_index == 0)
            m_FreeLook.m_XAxis.m_InvertInput = m_Toggles[0].isOn;
        else
            m_FreeLook.m_YAxis.m_InvertInput = m_Toggles[1].isOn;
    }

    public void OnBecomeActive()
    {
        m_Toggles[0].isOn = m_FreeLook.m_XAxis.m_InvertInput;
        m_Toggles[1].isOn = m_FreeLook.m_YAxis.m_InvertInput;
    }

}
