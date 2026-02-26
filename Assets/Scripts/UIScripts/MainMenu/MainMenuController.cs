using TMPro;
using UIScripts.Bootstrap;
using UnityEngine;

namespace UIScripts.MainMenu
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private SettingsMenuController _settingsMenuController;

        [SerializeField] private LevelBarController levelBarController;
    
        [SerializeField] private TMP_Text coinsText;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text levelText;

        [SerializeField] private Canvas canvas;
        [SerializeField] private GameObject mainMenu;
        [SerializeField] private GameObject settingsPopup;

        private void Update()
        {
            UpdateProfileUI();
        }

        private void UpdateProfileUI()
        {
            if (App.Instance.ProfileService != null)
            {
                coinsText.text = App.Instance.ProfileService.ProfileData.Coins.ToString();
                nameText.text = App.Instance.ProfileService.ProfileData.PlayerName.ToString();
                levelText.text = "Level " + App.Instance.ProfileService.ProfileData.Level.ToString();
                levelBarController.SetProgress(App.Instance.ProfileService.ProfileData.CurrentExperience, App.Instance.ProfileService.ProfileData.ExperienceToNextLevel);
            }
        }

        public void OnSettingsButtonPressed()
        {
            mainMenu.SetActive(false);
            _settingsMenuController.OpenSettings();
        }
    }
}
