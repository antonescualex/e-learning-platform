using System;
using UnityEngine;
using UnityEngine.UI;

public class MusicToggle : MonoBehaviour
{
    private AudioSource _musicAudioSource;

    private void Start()
    {
        _musicAudioSource = GameObject.Find("MusicManager").GetComponent<AudioSource>();

        if (_musicAudioSource == null)
        {
            Debug.Log("Nu a fost gasit audio sourceul");
        }
    }

    private void OnMusicToggleValueChanged(bool isMusicOn)
    {
        PlayMusic(isMusicOn);
    }
    
    public void OnMusicToggleValueChangedNullable(bool? isMusicOn)
    {
        if (!isMusicOn.HasValue)
        {
            Debug.Log("Switcherul trimite null");
            return;
        }

        PlayMusic(isMusicOn.Value);
    }

    private void PlayMusic(bool isMusicOn)
    {
        if (isMusicOn == null)
        {
            return;
        }

        _musicAudioSource.mute = !isMusicOn;
    }
}
