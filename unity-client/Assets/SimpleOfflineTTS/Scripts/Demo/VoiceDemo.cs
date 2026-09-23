using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleOfflineTTS
{
    public class VoiceDemo : MonoBehaviour
    {
        [SerializeField]
        TTSManager _ttsManager;

        [SerializeField]
        TTSVoice _ttsVoice;

        [SerializeField]
        TMP_Text _title;

        [SerializeField]
        TMP_Dropdown _voiceSelector;

        [SerializeField]
        bool _showLanguageSelector;

        [SerializeField]
        TMP_Dropdown _languageSelector;

        [SerializeField]
        TMP_InputField _inputField;

        [SerializeField]
        VUMeter _vuMeter;

        [SerializeField]
        Button _speakButton;

        [SerializeField]
        Button _saveToWavButton;

        [SerializeField]
        string _startingText = "Hello, world!";

        List<TTSDemo.SupertonicVoiceAndName> _supertonicVoices;
        List<TTSDemo.PiperVoiceAndConfig> _piperVoices;

        void Start()
        {
            Debug.AssertFormat(_ttsManager != null,         $"{nameof(_ttsManager)} is null in {this.name}");
            Debug.AssertFormat(_ttsVoice != null,           $"{nameof(_ttsVoice)} is null in {this.name}");

            Debug.AssertFormat(_title != null,              $"{nameof(_title)} is null in {this.name}");
            Debug.AssertFormat(_voiceSelector != null,      $"{nameof(_voiceSelector)} is null in {this.name}");
            Debug.AssertFormat(_languageSelector != null,   $"{nameof(_languageSelector)} is null in {this.name}");
            Debug.AssertFormat(_inputField != null,         $"{nameof(_inputField)} is null in {this.name}");
            Debug.AssertFormat(_vuMeter != null,            $"{nameof(_vuMeter)} is null in {this.name}");
            Debug.AssertFormat(_speakButton != null,        $"{nameof(_speakButton)} is null in {this.name}");
            Debug.AssertFormat(_saveToWavButton != null,    $"{nameof(_saveToWavButton)} is null in {this.name}");

            _voiceSelector.onValueChanged.AddListener(OnVoiceChanged);

            SetupLanguages();

            _inputField.SetTextWithoutNotify(_startingText);

            _speakButton.onClick.AddListener(OnSpeakButtonClicked);
            _saveToWavButton.onClick.AddListener(OnSaveToWavButtonClicked);
        }

        void Update()
        {
            if (_ttsVoice.IsSpeaking())
            {
                _vuMeter.SetRmsFromSource(_ttsVoice.GetAudioSource());
            }
            else
            {
                _vuMeter.Reset();
            }
        }

        public void SetupLanguages()
        {
            _languageSelector.gameObject.SetActive(_showLanguageSelector);

            _languageSelector.ClearOptions();

            int currentOption = 0;
            List<string> languageOptions = new List<string>();
            TTSVoice.Language[] supportedLanguages = _ttsVoice.GetSupportedLanguages();
            for (int i = 0; i < supportedLanguages.Length; i++)
            {
                languageOptions.Add(supportedLanguages[i].ToString());
                if (supportedLanguages[i] == _ttsVoice.GetLanguage())
                {
                    currentOption = i;
                }
            }
            _languageSelector.AddOptions(languageOptions);
            _languageSelector.SetValueWithoutNotify(currentOption);

            _languageSelector.onValueChanged.AddListener(OnLanguageChanged);
        }

        void OnLanguageChanged(int selectedIndex)
        {
            string currentLanguage = _languageSelector.captionText.text;
            TTSVoice.Language selectedLanguage = Enum.Parse<TTSVoice.Language>(currentLanguage);
            _ttsVoice.SetLanguage(selectedLanguage);
        }

        public void SetTitle(string title)
        {
            _title.text = title;
        }

        public void SetSupertonicVoices(List<TTSDemo.SupertonicVoiceAndName> voiceModels)
        {
            _supertonicVoices = voiceModels;

            int currentModelIndex = -1;

            _voiceSelector.ClearOptions();

            List<string> voiceOptions = new List<string>();
            for (int i = 0; i < voiceModels.Count; i++)
            {
                TTSDemo.SupertonicVoiceAndName voiceAndName = voiceModels[i];
                voiceOptions.Add(voiceAndName.Name);
                if (voiceAndName.Voice == _ttsVoice.GetSupertonicVoice())
                {
                    currentModelIndex = i;
                }
            }

            _voiceSelector.AddOptions(voiceOptions);
            _voiceSelector.SetValueWithoutNotify(currentModelIndex);
        }

        public void SetPiperVoiceModels(List<TTSDemo.PiperVoiceAndConfig> voiceModels)
        {
            _piperVoices = voiceModels;

            string currentVoiceName = _ttsVoice.GetModelName();
            int currentModelIndex = -1;

            _voiceSelector.ClearOptions();

            List<string> voiceOptions = new List<string>();
            for (int i = 0; i < voiceModels.Count; i++)
            {
                TTSDemo.PiperVoiceAndConfig voiceAndConfig = voiceModels[i];

                voiceOptions.Add(voiceAndConfig.Model.name);

                if (voiceAndConfig.Model.name == currentVoiceName)
                {
                    currentModelIndex = i;
                }
            }

            _voiceSelector.AddOptions(voiceOptions);
            _voiceSelector.SetValueWithoutNotify(currentModelIndex);
        }

        void OnVoiceChanged(int voiceIndex)
        {
            if (_supertonicVoices != null)
            {
                TTSDemo.SupertonicVoiceAndName voiceAndName = _supertonicVoices[voiceIndex];
                _ttsVoice.SetSupertonicVoice(voiceAndName.Voice);
            }
            else if (_piperVoices != null)
            {
                TTSDemo.PiperVoiceAndConfig voiceAndConfig = _piperVoices[voiceIndex];
                _ttsVoice.SetPiperModelAndConfig(voiceAndConfig.Model, voiceAndConfig.Config);
            }
        }

        public async void OnSpeakButtonClicked()
        {
            await _ttsVoice.Speak(_inputField.text);
        }

        public async void OnSaveToWavButtonClicked()
        {
#if UNITY_STANDALONE || UNITY_EDITOR
            AudioClip generatedClip = await _ttsVoice.GenerateAudioClip(_inputField.text);
            if (generatedClip == null)
            {
                Debug.LogError("No audio generated!");
                return;
            }

            string filePath = GetSaveFilePath("Save WAV File", "GeneratedAudio.wav", "wav");
            if (string.IsNullOrEmpty(filePath))
            {
                Debug.Log("Save cancelled.");
                return;
            }

            SaveWav(filePath, generatedClip);
            Debug.Log($"Saved audio to: {filePath}");
#else
            Debug.LogWarning("Audio saving is disabled on this platform.");
#endif
        }


#if UNITY_STANDALONE || UNITY_EDITOR
        private string GetSaveFilePath(string title, string defaultName, string extension)
        {
#if UNITY_EDITOR
            // Editor file picker
            string path = UnityEditor.EditorUtility.SaveFilePanel(title, "", defaultName, extension);
            return path;
#else
            // Basic OS-native save dialog for standalone builds (Windows/Linux/macOS)
            string defaultPath = Path.Combine(Application.persistentDataPath, defaultName);
            Debug.Log($"Saving to default path (no native dialog available): {defaultPath}");
            return defaultPath;
#endif
        }

        private void SaveWav(string path, AudioClip clip)
        {
            if (clip == null)
            {
                Debug.LogError("No AudioClip to save!");
                return;
            }

            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                int sampleCount = clip.samples * clip.channels;
                float[] samples = new float[sampleCount];
                clip.GetData(samples, 0);

                byte[] wavData = ConvertToWav(samples, clip.channels, clip.frequency);
                fileStream.Write(wavData, 0, wavData.Length);
            }
        }

        private byte[] ConvertToWav(float[] samples, int channels, int sampleRate)
        {
            int byteCount = samples.Length * 2; // 16-bit PCM
            using (MemoryStream stream = new MemoryStream(44 + byteCount))
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                // RIFF header
                writer.Write(System.Text.Encoding.UTF8.GetBytes("RIFF"));
                writer.Write(36 + byteCount);
                writer.Write(System.Text.Encoding.UTF8.GetBytes("WAVE"));

                // fmt subchunk
                writer.Write(System.Text.Encoding.UTF8.GetBytes("fmt "));
                writer.Write(16);
                writer.Write((ushort)1); // PCM format
                writer.Write((ushort)channels);
                writer.Write(sampleRate);
                writer.Write(sampleRate * channels * 2);
                writer.Write((ushort)(channels * 2));
                writer.Write((ushort)16);

                // data subchunk
                writer.Write(System.Text.Encoding.UTF8.GetBytes("data"));
                writer.Write(byteCount);

                // Write samples
                foreach (float sample in samples)
                {
                    short intData = (short)(Mathf.Clamp(sample, -1f, 1f) * short.MaxValue);
                    writer.Write(intData);
                }

                return stream.ToArray();
            }
        }
#endif
    }
}
