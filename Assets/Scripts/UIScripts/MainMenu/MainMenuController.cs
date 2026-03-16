using System;
using System.Collections;
using Data.StaticData;
using Data.StaticData.Avatar;
using Services;
using TMPro;
using UIScripts.MainMenu.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace UIScripts.MainMenu
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private SettingsMenuController settingsMenuController;
        [SerializeField] private LevelBarController levelBarController;

        [SerializeField] private TMP_Text coinsText;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text levelText;

        [SerializeField] private Canvas canvas;
        [SerializeField] private GameObject mainMenu;
        [SerializeField] private GameObject settingsPopup;

        [Header("Avatar")]
        [SerializeField] private Image profileImage;
        [SerializeField] private AvatarCatalog avatarCatalog;

        private IProfileService _profileService;

        private void Start()
        {
            _profileService = App.Instance.ProfileService;
        }

        private void OnEnable()
        {
            if (_profileService == null && App.Instance != null)
            {
                _profileService = App.Instance.ProfileService;
            }

            if (_profileService != null)
            {
                _profileService.ProfileChanged += OnProfileChanged;
                StartCoroutine(RefreshAfterOneFrame());
            }
        }

        private IEnumerator RefreshAfterOneFrame()
        {
            yield return null;
            if (_profileService != null && _profileService.HasProfile)
            {
                OnProfileChanged(_profileService.ProfileData);
            }
        }

        private void OnDisable()
        {
            if (_profileService != null)
            {
                _profileService.ProfileChanged -= OnProfileChanged;
            }
        }

        private void OnProfileChanged(ProfileData profileData)
        {
            if (profileData == null) return;

            coinsText.text = profileData.Coins.ToString();
            nameText.text = profileData.PlayerName;
            levelText.text = "Level " + profileData.Level;

            levelBarController.SetProgress(profileData.CurrentExperience, profileData.ExperienceToNextLevel);

            var sprite = avatarCatalog.GetSprite(profileData.AvatarId);
            if (sprite != null) profileImage.sprite = sprite;
        }

        public void OnSettingsButtonPressed()
        {
            mainMenu.SetActive(false);
            settingsMenuController.OpenSettings();
        }
    }
}
