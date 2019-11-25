using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Tobias Stroedicke

public class Pause : MonoBehaviour {

    public PlayerEntity m_Player;
#pragma warning disable 0649
    [SerializeField]
    private GameObject m_BackGround;
#pragma warning restore

    public GameObject BackGround { get { return m_BackGround; } }
#pragma warning disable 0649
    /// <summary>This gameobject is the first one you see when you press pause</summary>
    [SerializeField]
    private GameObject m_PauseEnter;
#pragma warning restore

    /// <summary>This gameobject is the first one you see when you press pause</summary>
    public GameObject PauseEnter { get { return m_PauseEnter; } }
#pragma warning disable 0649
    [SerializeField]
    private GameObject m_Options;
#pragma warning restore

    public GameObject Options { get { return m_Options; } }

#pragma warning disable 0649
    [Header("Specific Pause Enter Variables")]
    [SerializeField]
    private Button m_ExitButton;
#pragma warning restore


#pragma warning disable 0649
    [Header("Option Variables")]
    [SerializeField]
    private Sound opt_Sound;
#pragma warning restore

#pragma warning disable 0649
    [SerializeField]
    private CameraSpeedX opt_CameraSpeedX;
#pragma warning restore

#pragma warning disable 0649
    [SerializeField]
    private CameraSpeedY opt_CameraSpeedY;
#pragma warning restore

#pragma warning disable 0649
    [SerializeField]
    private CameraInvert opt_CameraInvert;
#pragma warning restore
    /// <summary>
    /// Calls when pause menu shall open
    /// </summary>
	public void CallPauseEnter()
    {
        BackGround.SetActive(true);
        Options.SetActive(false);
        PauseEnter.SetActive(true);

        Button.ButtonClickedEvent clickEvent = new Button.ButtonClickedEvent();
        clickEvent.AddListener(() => GameObject.Find("Network Manager").GetComponent<MyNetworkManager>().CloseConnection(m_Player));
        m_ExitButton.onClick = clickEvent;
    }

    /// <summary>
    /// Calls when user opens option menu
    /// </summary>
    public void CallOptions()
    {
        PauseEnter.SetActive(false);
        Options.SetActive(true);
        opt_Sound.OnBecomeActive();
        opt_CameraSpeedX.OnBecomeActive();
        opt_CameraSpeedY.OnBecomeActive();
        opt_CameraInvert.OnBecomeActive();
    }

    /// <summary>
    /// Quit Pause Menu
    /// </summary>
    public void PlayerResuming()
    {
        Options.SetActive(false);
        PauseEnter.SetActive(true);
    }

}
