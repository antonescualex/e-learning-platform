using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace SimpleOfflineTTS
{
    [Serializable]
    public class AudioConfig
    {
        public int sample_rate;
        public string quality;
    }

    [Serializable]
    public class EspeakConfig
    {
        public string voice;
    }

    [Serializable]
    public class InferenceConfig
    {
        public float noise_scale;
        public float length_scale;
        public float noise_w;
    }

    [Serializable]
    public class LanguageConfig
    {
        public string code;
        public string family;
        public string region;
        public string name_native;
        public string name_english;
        public string country_english;
    }

    [Serializable]
    public class PiperModelConfig
    {
        public AudioConfig audio;
        public EspeakConfig espeak;
        public InferenceConfig inference;
        public string phoneme_type;

        // These can be loaded directly as dictionaries in Newtonsoft
        public Dictionary<string, int[]> phoneme_map;
        public Dictionary<string, int[]> phoneme_id_map;
        public int num_symbols;
        public int num_speakers;
        public Dictionary<string, int[]> speaker_id_map;

        public string piper_version;
        public LanguageConfig language;
        public string dataset;

        private Dictionary<string, int> _phonemeIdMap;

        public static PiperModelConfig Load(TextAsset configTextAsset)
        {
            if (configTextAsset == null)
            {
                Debug.LogError("Trying to load a null Piper model config.");
                return null;
            }

            try
            {
                var config = JsonConvert.DeserializeObject<PiperModelConfig>(configTextAsset.text);

                // Build the convenience lookup table (string ? int)
                config._phonemeIdMap = new Dictionary<string, int>();

                if (config.phoneme_id_map != null)
                {
                    foreach (var kv in config.phoneme_id_map)
                    {
                        if (kv.Value != null && kv.Value.Length > 0)
                            config._phonemeIdMap[kv.Key] = kv.Value[0];
                    }
                }

                Debug.Log($"PiperModelConfig loaded: {config._phonemeIdMap.Count} phoneme IDs parsed.");
                return config;
            }
            catch (Exception ex)
            {
                Debug.LogError($"PiperModelConfig: Failed to parse JSON: {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }

        public Dictionary<string, int> GetPhonemeIdMap()
        {
            return _phonemeIdMap;
        }
    }
}
