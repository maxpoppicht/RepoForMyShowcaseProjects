using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Tobias Stroedicke

public class CameraSpeedX : MonoBehaviour {
#pragma warning disable 0649
    [SerializeField]
    private Slider m_Slider;
#pragma warning restore

    [SerializeField]
    private Text m_Text;
#pragma warning disable 0649
    [SerializeField]
    private InputField m_Input;
#pragma warning restore
#pragma warning disable 0649
    [SerializeField]
    private Cinemachine.CinemachineFreeLook m_FreeLook;
#pragma warning restore

    private RectTransform m_rect;
    private RectTransform Rect
    {
        get
        {
            if (m_rect == null)
            {
                m_rect = GetComponent<RectTransform>();
            }
            return m_rect;
        }
    }
    
    /// <summary>
    /// Change slider value on input box
    /// </summary>
    public void ChangeSliderValue()
    {
        int newValue;
        bool work = int.TryParse(m_Input.text, out newValue);

        if (!work)
            return;

        m_Slider.value = newValue;
    }

    public void ChangeInputValue()
    {
        m_Input.text = m_Slider.value.ToString();
        m_FreeLook.m_XAxis.m_MaxSpeed = m_Slider.value;
    }

    public void OnBecomeActive()
    {
        m_Slider.value = m_FreeLook.m_XAxis.m_MaxSpeed;
        m_Input.text = m_FreeLook.m_XAxis.m_MaxSpeed.ToString();
    }

    private void Update()
    {
        float x = -(Screen.width / 2);
        float y = -(Screen.height / 5);
        transform.position = new Vector3(Screen.width / 4, transform.position.y, transform.position.z);
        Rect.sizeDelta = new Vector2(x, y);
    }
}
