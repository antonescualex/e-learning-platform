using Data;
using UnityEngine;
using UnityEngine.UI;

namespace UIScripts.MainMenu.Settings
{
    public class SettingsView : MonoBehaviour
    {
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider soundVolumeSlider;
        [SerializeField] private Slider musicToggle;
        [SerializeField] private Slider soundToggle;

        private SettingsMenuController _settingsMenuController;
        private SettingsData _originalData;
        private SettingsData _editedData;
        private bool _ignoreEvents;

        public void Init(SettingsMenuController settingsMenuController, SettingsData currentData)
        {
            _settingsMenuController = settingsMenuController;
            _originalData = currentData.Copy();
            _editedData = currentData.Copy();
        
            SetUiValues(_editedData);
            HookEvents();
        }

        private void SetUiValues(SettingsData settingsData)
        {
            _ignoreEvents = true;

            musicVolumeSlider.value = settingsData.MusicVolume;
            soundVolumeSlider.value = settingsData.SfxVolume;

            musicToggle.value = settingsData.MusicEnabled ? 1f : 0f;
            soundToggle.value = settingsData.SfxEnabled ? 1f : 0f;

            _ignoreEvents = false;
        }

        private void HookEvents()
        {
            musicVolumeSlider.onValueChanged.AddListener(_ =>OnUiChanged());
            soundVolumeSlider.onValueChanged.AddListener(_ =>OnUiChanged());
            musicToggle.onValueChanged.AddListener(_ =>OnUiChanged());
            soundToggle.onValueChanged.AddListener(_ =>OnUiChanged());
        }
    
        private void OnUiChanged()
        {
            if (_ignoreEvents) return;

            _editedData.MusicVolume = musicVolumeSlider.value;
            _editedData.SfxVolume = soundVolumeSlider.value;
            _editedData.MusicEnabled = musicToggle.value >= 0.5f;
            _editedData.SfxEnabled = soundToggle.value >= 0.5f;
        
            _settingsMenuController.Preview(_editedData);
        }
    
        public void OnSave() => _settingsMenuController.Save(_editedData);

        public void OnCancel() => _settingsMenuController.Cancel(_originalData);
    }
}
