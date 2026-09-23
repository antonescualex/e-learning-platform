using App;
using Data;
using Services;
using Services.Interfaces;
using UIScripts.Bootstrap;
using UnityEngine;

namespace UIScripts.MainMenu.Settings
{
    public class SettingsMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject settingsPopup;
        [SerializeField] private GameObject mainMenu;
        [SerializeField] private Canvas canvas;

        private GameObject currentPopup;

        private ISettingsService _settingsService;

        private void Start()
        {
            ServiceContainer.TryResolve<ISettingsService>(out _settingsService);
        }

        public void OpenSettings()
        {
            if (currentPopup != null) return;

            mainMenu.SetActive(false);

            currentPopup = Instantiate(settingsPopup, canvas.transform);
            currentPopup.transform.SetAsLastSibling();

            var popup = currentPopup.GetComponent<SettingsView>();
            popup.Init(this, _settingsService.CurrentSettings.Copy());

            currentPopup.GetComponent<Ricimi.Popup>()?.Open();
        }

        public void Preview(SettingsData editedSettings)
        {
            AudioManager.Instance?.ApplySettings(editedSettings);
        }

        public void Save(SettingsData editedSettings)
        {
            if (_settingsService == null) return;

            _settingsService.Save(editedSettings);
            AudioManager.Instance?.ApplySettings(_settingsService.CurrentSettings);
            Close();
        }

        public void Cancel(SettingsData originalSettings)
        {
            AudioManager.Instance?.ApplySettings(originalSettings);
            Close();
        }

        private void Close()
        {
            mainMenu.SetActive(true);

            if (currentPopup != null)
            {
                currentPopup.GetComponent<Ricimi.Popup>()?.Close();
                currentPopup = null;
            }
        }
    }
}
