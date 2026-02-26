using UnityEngine;

namespace UIScripts.Bootstrap
{
    public class SettingsMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject settingsPopup;
        [SerializeField] private GameObject mainMenu;
        [SerializeField] private Canvas canvas;

        private GameObject currentPopup;

        public void OpenSettings()
        {
            if (currentPopup != null) return;
            
            mainMenu.SetActive(false);

            currentPopup = Instantiate(settingsPopup, canvas.transform);
            currentPopup.transform.SetAsLastSibling();

            var popup = currentPopup.GetComponent<SettingsPopup>();
            popup.Init(this, App.Instance.SettingsService.CurrentSettings.Copy());
            
            currentPopup.GetComponent<Ricimi.Popup>()?.Open();
        }

        public void Preview(SettingsData editedSettings)
        {
            AudioManager.Instance?.ApplySettings(editedSettings);
        }

        public void Save(SettingsData editedSettings)
        {
            App.Instance.SettingsService.Save(editedSettings);
            AudioManager.Instance?.ApplySettings(App.Instance.SettingsService.CurrentSettings);
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