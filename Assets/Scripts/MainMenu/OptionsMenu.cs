using System;
using UISwitcher;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private UISwitcher.UISwitcher musicToggle;
    
    private void Awake()
    {
        volumeSlider.value = AudioManager.Instance.CurrentVolume;
        volumeSlider.onValueChanged.AddListener(OnVolumeSliderChanged);
        
        musicToggle.isOn = !AudioManager.Instance.IsMuted;
        musicToggle.onValueChanged.AddListener(OnMusicToggleChanged);
    }

    private void OnMusicToggleChanged(bool isMuted)
    {
        AudioManager.Instance.SetMuted(!isMuted);
    }

    private void OnVolumeSliderChanged(float volume)
    {
        AudioManager.Instance.AdjustVolume(volume);
    }
}
