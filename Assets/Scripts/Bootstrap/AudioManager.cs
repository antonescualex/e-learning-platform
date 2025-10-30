using System;
using UnityEngine;

public class AudioManager : MonoBehaviourSingleton<AudioManager>
{
    //TODO: Save audio settings
    
    public enum MusicTypes
    {
        Background
    }
    
    public enum SfxTypes
    {
        ButtonClick
    }
    
    [SerializeField] private AudioSource backgroundMusicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip backgroundMusicClip;
    [SerializeField] private AudioClip buttonClickClip;

    public float CurrentVolume { get; private set; }
    public bool IsMuted { get; private set; }

    public void Initialize(float volume, bool isMuted)
    {
        CurrentVolume = volume;
        IsMuted = isMuted;
    }

    public override void Awake()
    {
        base.Awake();
        CurrentVolume = 0.15f;
        IsMuted = false;
    }

    public void PlayMusic(MusicTypes musicType)
    {
        AudioClip audioClip = null;
        switch (musicType)
        {
            case MusicTypes.Background:
                audioClip = backgroundMusicClip;
                break;
        }

        backgroundMusicSource.clip = audioClip;
        backgroundMusicSource.Play();
    }
    
    public void PlaySfx(SfxTypes sfxType)
    {
        AudioClip audioClip = null;
        switch (sfxType)
        {
            case SfxTypes.ButtonClick:
                audioClip = buttonClickClip;
                break;
        }
        
        sfxSource.PlayOneShot(audioClip);
    }

    public void AdjustVolume(float volume)
    {
        
        CurrentVolume = volume;
        sfxSource.volume = volume;
        backgroundMusicSource.volume = volume;
    }

    public void SetMuted(bool isMuted)
    {
        if (isMuted)
        {
            backgroundMusicSource.Pause();
        }
        else
        {
            backgroundMusicSource.Play();
        }
    }
    
}
