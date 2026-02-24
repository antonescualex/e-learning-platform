using System;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider musicToggle;
    
    private void Awake()
    {
        volumeSlider.value = AudioManager.Instance.CurrentVolume;
        volumeSlider.onValueChanged.AddListener(OnVolumeSliderChanged);
        
        // musicToggle.isOn = !AudioManager.Instance.IsMuted;
        musicToggle.onValueChanged.AddListener(OnMusicToggleChanged);
    }

    private void OnMusicToggleChanged(float value)
    {
        AudioManager.Instance.SetMuted(!value.Equals(1));
    }

    private void OnVolumeSliderChanged(float volume)
    {
        AudioManager.Instance.AdjustVolume(volume);
    }
}
