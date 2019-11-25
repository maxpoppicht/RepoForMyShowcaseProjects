using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

// Tobias Stroedicke

public class Sound : MonoBehaviour {
    [Header("Main Sound")]
#pragma warning disable 0649
    [SerializeField]
    private Slider m_MainSoundSlider;
#pragma warning restore

    [SerializeField]
    private Text m_MainSoundText;
#pragma warning disable 0649
    [SerializeField]
    private AudioSource m_MainSoundAudio;
#pragma warning restore

    [Header("Sound Effect")]
#pragma warning disable 0649
    [SerializeField]
    private Slider m_SoundEffectSlider;
#pragma warning restore

#pragma warning disable 0649
    [SerializeField]
    private Text m_SoundEffectText;
#pragma warning restore

#pragma warning disable 0649
    [SerializeField]
    private AudioSource[] m_SoundEffectAudios;
#pragma warning restore

    /// <summary>
    /// Set Music sound volume
    /// </summary>
    public void SetMainSound()
    {
        m_MainSoundAudio.volume = m_MainSoundSlider.value;
    }

    /// <summary>
    /// Set Sound Effect volume
    /// </summary>
    public void SetEffectSound()
    {
        for (int i = 0; i < m_SoundEffectAudios.Length; i++)
        {
            m_SoundEffectAudios[i].volume = m_SoundEffectSlider.value;
        }
    }

    public void OnBecomeActive()
    {
        // Set Slider Value
        m_MainSoundSlider.value = m_MainSoundAudio.volume;
        m_SoundEffectSlider.value = m_SoundEffectAudios[0].volume;
    }
}
