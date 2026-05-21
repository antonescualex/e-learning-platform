using System;
using System.Collections.Generic;
using TMPro;
using Unity.InferenceEngine;
using UnityEngine;
using UnityEngine.Diagnostics;

namespace SimpleOfflineTTS
{
    public class TTSDemo : MonoBehaviour
    {
        [Serializable]
        public class SupertonicVoiceAndName
        {
            public SupertonicVoice Voice;
            public string Name;
        }

        [Serializable]
        public class PiperVoiceAndConfig
        {
            public ModelAsset Model;
            public TextAsset Config;
        }

        [SerializeField]
        List<SupertonicVoiceAndName> _supertonicVoiceModels;

        [SerializeField]
        List<PiperVoiceAndConfig> _piperVoiceModels;

        [SerializeField]
        TMP_Dropdown _backendDropdown;

        [SerializeField]
        TTSManager _ttsManager;

        [SerializeField]
        List<VoiceDemo> _voiceDemos;

        bool _populatedBackends = false;

        private void Start()
        {
            Debug.AssertFormat(_piperVoiceModels != null,   $"{nameof(_piperVoiceModels)} is null in {this.name}");
            Debug.AssertFormat(_backendDropdown != null,    $"{nameof(_backendDropdown)} is null in {this.name}");
            Debug.AssertFormat(_ttsManager != null,         $"{nameof(_ttsManager)} is null in {this.name}");
            Debug.AssertFormat(_voiceDemos != null,         $"{nameof(_voiceDemos)} is null in {this.name}");

            // Use high framerate to better show inference times in the demo - not necessary for your project
            SetHighFramerate();

            // Clear backend dropdown (populate when TTSManager is ready)
            _backendDropdown.ClearOptions();

            if ((_voiceDemos.Count > 0) && (_voiceDemos[0] != null))
            {
                _voiceDemos[0].SetTitle("Supertonic Voice");
                _voiceDemos[0].SetSupertonicVoices(_supertonicVoiceModels);
            }

            if ((_voiceDemos.Count > 1) && (_voiceDemos[1] != null))
            {
                _voiceDemos[1].SetTitle("Piper Voice");
                _voiceDemos[1].SetPiperVoiceModels(_piperVoiceModels);
            }
        }

        void SetHighFramerate()
        {
            // Pick highest rate
            var rates = Screen.resolutions;
            Resolution current = Screen.currentResolution;

            Screen.SetResolution(
                current.width,
                current.height,
                FullScreenMode.FullScreenWindow,
                current.refreshRateRatio
            );

            Application.targetFrameRate = (int)current.refreshRateRatio.value;

            Debug.Log($"Set framerate to {current.refreshRateRatio.value} for TTSDemo.");
        }

        void Update()
        {
            if (_ttsManager.IsReady())
            {
                if (!_populatedBackends)
                {
                    PopulateBackends();
                }
            }
        }

        void PopulateBackends()
        {
            BackendType currentBackend = _ttsManager.GetCurrentBackend();
            int currentBackendIndex = -1;

            List<string> backendOptions = new List<string>();
            Array backends = Enum.GetValues(typeof(BackendType));
            for (int i = 0; i < backends.Length; i++)
            {
                backendOptions.Add(backends.GetValue(i).ToString());

                if (((BackendType)backends.GetValue(i)) == currentBackend)
                {
                    currentBackendIndex = i;
                }
            }
            _backendDropdown.AddOptions(backendOptions);
            _backendDropdown.SetValueWithoutNotify(currentBackendIndex);
            _backendDropdown.onValueChanged.AddListener(OnBackendSelected);

            _populatedBackends = true;
        }

        async void OnBackendSelected(int selectedValue)
        {
            string backendName = _backendDropdown.captionText.text;
            BackendType selectedBackend = Enum.Parse<BackendType>(backendName);

            await _ttsManager.SetBackendType(selectedBackend);
        }
    }
}
