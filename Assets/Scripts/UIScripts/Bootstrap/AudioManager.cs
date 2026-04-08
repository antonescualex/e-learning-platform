using Data;
using UnityEngine;
using UnityEngine.Audio;

namespace UIScripts.Bootstrap
{
    public class AudioManager : MonoBehaviourSingleton<AudioManager>
    {
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioClip backgroundMusicClip;

        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private string musicVolumeParam = "MusicVolume";
        [SerializeField] private string sfxVolumeParam = "SfxVolume";

        private void Start()
        {
            if (musicSource != null && backgroundMusicClip != null)
            {
                musicSource.clip = backgroundMusicClip;
                musicSource.loop = true;
                musicSource.Play();
            }
        }

        public void ApplySettings(SettingsData settings)
        {
            ApplyMixerVolume(musicVolumeParam, settings.MusicEnabled, settings.MusicVolume);
            ApplyMixerVolume(sfxVolumeParam, settings.SfxEnabled, settings.SfxVolume);

            if (musicSource != null)
            {
                if(!settings.MusicEnabled && musicSource.isPlaying) musicSource.Pause();
                if(settings.MusicEnabled && !musicSource.isPlaying) musicSource.Play();
            }
        }

        private void ApplyMixerVolume(string param, bool enabled, float value)
        {
            if (audioMixer == null) return;

            float db;
            if (!enabled)
            {
                db = -80f;
            }
            else
            {
                value = Mathf.Clamp01(value);
                db = (value <= 0.0001f) ? -80f : Mathf.Log10(value) * 20f;
            }

            audioMixer.SetFloat(param, db);
        }
    }
}
