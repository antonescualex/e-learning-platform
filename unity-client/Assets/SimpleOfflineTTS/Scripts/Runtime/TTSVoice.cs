using System;
using System.Threading.Tasks;
using Unity.InferenceEngine;
using UnityEngine;

namespace SimpleOfflineTTS
{
    public class TTSVoice : MonoBehaviour
    {
        public enum Language
        {
            English, Korean, Japanese, Arabic,
            Bulgarian, Czech, Danish, German,
            Greek, Spanish, Estonian, Finnish,
            French, Hindi, Croatian, Hungarian,
            Indonesian, Italian, Lithuanian, Latvian,
            Dutch, Polish, Portuguese, Romanian,
            Russian, Slovak, Slovenian, Swedish,
            Turkish, Ukrainian, Vietnamese
        }

        [SerializeField, Tooltip("Reference to your TTSManager script.")]
        TTSManager _ttsManager;

        [Header("Supertonic Settings - Needed if you're using Supertonic Models (recommended)")]
        [SerializeField, Tooltip("Supertonic Voice Style - drag one of the F1...F5 or M1...M5 (or your own) voices here.")]
        SupertonicVoice _supertonicVoice;

        [SerializeField, Range(2, 16), Tooltip("Quality of Supertonic voice reproduction. More steps = better, more demanding.")]
        int _supertonicSteps = 10;

        [SerializeField, Range(0.8f, 1.3f), Tooltip("Voice speed, 1.0 = standard")]
        float _supertonicVoiceSpeed = 1.0f;

        [SerializeField, Tooltip("Language")]
        Language _supertonicLanguage = Language.English;

        [Header("Piper Settings - Only needed if you're using Piper TTS Models")]
        [SerializeField, Tooltip("The Piper speech model, in .onnx format.")]
        ModelAsset _modelAsset;

        [SerializeField, Tooltip("The .json config file that accompanies a .onnx Piper model.")]
        TextAsset _modelConfig;

        [SerializeField, Tooltip("Warm up the model before use, to avoid initial latency.")]
        bool _piperWarmup = true;

        [Header("Audio Playback Settings")]
        [SerializeField, Tooltip("Optional: AudioClip used for playback. Created at runtime if null.")]
        AudioSource _audioSource;

        public int Steps
        {
            get { return _supertonicSteps; }
            set { _supertonicSteps = value; }
        }

        public float Speed
        {
            get { return _supertonicVoiceSpeed; }
            set { _supertonicVoiceSpeed = value; }
        }

        PiperModelConfig _piperModelConfig;
        bool _piperModelWarmupStarted;
        bool _piperModelWarmedUp;

        void Start()
        {
            Debug.AssertFormat(_ttsManager != null, $"TTSManager is null in {this.name}", this);

            if (_supertonicVoice != null)
            {
                _supertonicVoice.CreateStyleTensor();
            }
            else if (_modelAsset != null)
            {
                _piperModelConfig = PiperModelConfig.Load(_modelConfig);
                _piperModelWarmupStarted = false;
                _piperModelWarmedUp = false;
            }
            else
            {
                Debug.LogError("TTSVoice requires either a Supertonic Voice or Piper Model.", this);
            }
        }

        void Update()
        {
            if ((_modelAsset != null) && _piperWarmup && !_piperModelWarmedUp && !_piperModelWarmupStarted)
            {
                if (_ttsManager.IsReady())
                {
                    _piperModelWarmupStarted = true;
                    _ttsManager.TextToSpeechPiper("Hello world", _modelAsset, _piperModelConfig).ContinueWith(_ =>
                    {
                        _piperModelWarmedUp = true;
                        Debug.Log("Piper model warmup completed.");
                    });
                }
            }
        }

        void OnDestroy()
        {
            _supertonicVoice?.Dispose();
        }

        public void SetSupertonicVoice(SupertonicVoice voice)
        {
            _supertonicVoice = voice;
            _supertonicVoice.CreateStyleTensor();
        }

        public SupertonicVoice GetSupertonicVoice()
        {
            return _supertonicVoice;
        }

        public void SetPiperModelAndConfig(ModelAsset modelAsset, TextAsset modelConfig)
        {
            _modelAsset = modelAsset;
            _modelConfig = modelConfig;

            _piperModelConfig = PiperModelConfig.Load(_modelConfig);

            _piperModelWarmupStarted = false;
            _piperModelWarmedUp = false;
        }

        public string GetModelName()
        {
            return _modelAsset.name;
        }

        public PiperModelConfig GetModelConfig()
        {
            return _piperModelConfig;
        }

        public Language[] GetSupportedLanguages()
        {
            if (_supertonicVoice != null)
            {
                return new Language[]
                {
                    Language.English, Language.Korean, Language.Japanese, Language.Arabic,
                    Language.Bulgarian, Language.Czech, Language.Danish, Language.German,
                    Language.Greek, Language.Spanish, Language.Estonian, Language.Finnish,
                    Language.French, Language.Hindi, Language.Croatian, Language.Hungarian,
                    Language.Indonesian, Language.Italian, Language.Lithuanian, Language.Latvian,
                    Language.Dutch, Language.Polish, Language.Portuguese, Language.Romanian,
                    Language.Russian, Language.Slovak, Language.Slovenian, Language.Swedish,
                    Language.Turkish, Language.Ukrainian, Language.Vietnamese
                };
            }
            else if (_piperModelConfig != null)
            {
                return new Language[] { Language.English };
            }
            else
            {
                return Array.Empty<Language>();
            }
        }

        public void SetLanguage(Language language)
        {
            _supertonicLanguage = language;
        }

        public Language GetLanguage()
        {
            return _supertonicLanguage;
        }

        void GetSupertonicLanguageTags(out string openTag, out string closeTag)
        {
            string languageTag = GetLanguageCode(_supertonicLanguage);

            openTag = "<" + languageTag + ">";
            closeTag = "</" + languageTag + ">";
        }

        static string GetLanguageCode(Language language)
        {
            switch (language)
            {
                case Language.English:
                    return "en";
                case Language.Korean:
                    return "ko";
                case Language.Japanese:
                    return "ja";
                case Language.Arabic:
                    return "ar";
                case Language.Bulgarian:
                    return "bg";
                case Language.Czech:
                    return "cs";
                case Language.Danish:
                    return "da";
                case Language.German:
                    return "de";
                case Language.Greek:
                    return "el";
                case Language.Spanish:
                    return "es";
                case Language.Estonian:
                    return "et";
                case Language.Finnish:
                    return "fi";
                case Language.French:
                    return "fr";
                case Language.Hindi:
                    return "hi";
                case Language.Croatian:
                    return "hr";
                case Language.Hungarian:
                    return "hu";
                case Language.Indonesian:
                    return "id";
                case Language.Italian:
                    return "it";
                case Language.Lithuanian:
                    return "lt";
                case Language.Latvian:
                    return "lv";
                case Language.Dutch:
                    return "nl";
                case Language.Polish:
                    return "pl";
                case Language.Portuguese:
                    return "pt";
                case Language.Romanian:
                    return "ro";
                case Language.Russian:
                    return "ru";
                case Language.Slovak:
                    return "sk";
                case Language.Slovenian:
                    return "sl";
                case Language.Swedish:
                    return "sv";
                case Language.Turkish:
                    return "tr";
                case Language.Ukrainian:
                    return "uk";
                case Language.Vietnamese:
                    return "vi";
                default:
                    return "en";
            }
        }

        public Task<AudioClip> GenerateAudioClip(string text)
        {
            if (_supertonicVoice != null)
            {
                GetSupertonicLanguageTags(out string openLanguageTag, out string closeLanguageTag);
                string wrappedText = openLanguageTag + text + closeLanguageTag;
                return _ttsManager.TextToSpeechSupertonic(wrappedText, _supertonicVoice, _supertonicSteps, _supertonicVoiceSpeed);
            }
            else if (_piperModelConfig != null)
            {
                return _ttsManager.TextToSpeechPiper(text, _modelAsset, _piperModelConfig);
            }
            else
            {
                Debug.LogError("TTSVoice requires either a Supertonic Voice or Piper Model, please add one in the Inspector.", this);
                return Task.FromResult<AudioClip>(null);
            }
        }

        public async Task Speak(string text)
        {
            AudioClip generatedClip = await GenerateAudioClip(text);

            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.loop = false;
            }

            _audioSource.clip = generatedClip;
            _audioSource.Play();
        }

        public void Speak(AudioClip clip)
        {
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.loop = false;
            }

            _audioSource.clip = clip;
            _audioSource.Play();
        }

        public bool IsSpeaking()
        {
            if (_audioSource == null)
            {
                return false;
            }

            return _audioSource.isPlaying;
        }

        public void StopSpeaking()
        {
            if (_audioSource == null)
            {
                return;
            }

            _audioSource.Stop();
        }

        public AudioSource GetAudioSource()
        {
            return _audioSource;
        }
    }
}
