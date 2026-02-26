using System;
using UnityEngine;

[Serializable]
public class SettingsData
{
    public bool MusicEnabled = true;
    public bool SfxEnabled = true;
    
    [Range(0f, 1f)] public float MusicVolume = 0.2f;
    [Range(0f, 1f)] public float SfxVolume = 0.2f;

    public SettingsData Copy()
    {
        return new SettingsData
        {
            MusicEnabled = MusicEnabled,
            MusicVolume = MusicVolume,
            SfxEnabled = SfxEnabled,
            SfxVolume = SfxVolume
        };
    }
}
