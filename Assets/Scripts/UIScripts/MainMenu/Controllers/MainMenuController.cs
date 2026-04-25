using System.Collections;
using App;
using Data;
using Data.StaticData.Avatar;
using Data.StaticData.Item;
using Services.Interfaces;
using TMPro;
using UIScripts.MainMenu.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace UIScripts.MainMenu.Controllers
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
        [SerializeField] private GameObject badgeInfoPrefab;
        [SerializeField] private float badgeInfoDurationSeconds = 3.5f;

        [Header("Avatar")]
        [SerializeField] private Image profileImage;
        [SerializeField] private AvatarCatalog avatarCatalog;

        private IProfileService _profileService;
        private IBadgeService _badgeService;
        private Coroutine _badgeInfoRoutine;
        private GameObject _currentBadgeInfo;
        private bool _isShowingBadgeInfo;

        private void Start()
        {
            ServiceContainer.TryResolve<IProfileService>(out _profileService);
            ServiceContainer.TryResolve<IBadgeService>(out _badgeService);
        }

        private void OnEnable()
        {
            if (_profileService == null)
            {
                ServiceContainer.TryResolve<IProfileService>(out _profileService);
            }

            if (_badgeService == null)
            {
                ServiceContainer.TryResolve<IBadgeService>(out _badgeService);
            }

            if (_profileService != null)
            {
                _profileService.ProfileChanged += OnProfileChanged;
                StartCoroutine(RefreshAfterOneFrame());
            }

            if (_badgeService != null)
            {
                _badgeService.NotificationsAvailable += OnBadgeNotificationsAvailable;
                StartCoroutine(ShowPendingBadgesNextFrame());
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

            if (_badgeService != null)
            {
                _badgeService.NotificationsAvailable -= OnBadgeNotificationsAvailable;
            }

            if (_badgeInfoRoutine != null)
            {
                StopCoroutine(_badgeInfoRoutine);
                _badgeInfoRoutine = null;
            }

            _isShowingBadgeInfo = false;

            if (_currentBadgeInfo != null)
            {
                Destroy(_currentBadgeInfo);
                _currentBadgeInfo = null;
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

        private IEnumerator ShowPendingBadgesNextFrame()
        {
            yield return null;
            TryShowNextBadgeInfo();
        }

        private void OnBadgeNotificationsAvailable()
        {
            TryShowNextBadgeInfo();
        }

        private void TryShowNextBadgeInfo()
        {
            if (_isShowingBadgeInfo) return;
            if (_badgeService == null || badgeInfoPrefab == null || canvas == null) return;
            if (!_badgeService.TryDequeueNotification(out BadgeDefinition badgeDefinition)) return;

            _badgeInfoRoutine = StartCoroutine(ShowBadgeInfo(badgeDefinition));
        }

        private IEnumerator ShowBadgeInfo(BadgeDefinition badgeDefinition)
        {
            _isShowingBadgeInfo = true;

            _currentBadgeInfo = Instantiate(badgeInfoPrefab, canvas.transform, false);
            _currentBadgeInfo.transform.SetAsLastSibling();

            TMP_Text descriptionText = FindBadgeInfoDescription(_currentBadgeInfo.transform);
            if (descriptionText != null)
            {
                string badgeName = badgeDefinition != null && !string.IsNullOrWhiteSpace(badgeDefinition.DisplayName)
                    ? badgeDefinition.DisplayName
                    : "new";
                descriptionText.text = $"You have received {badgeName} badge!";
            }

            yield return new WaitForSecondsRealtime(Mathf.Max(0.1f, badgeInfoDurationSeconds));

            if (_currentBadgeInfo != null)
            {
                Destroy(_currentBadgeInfo);
                _currentBadgeInfo = null;
            }

            _isShowingBadgeInfo = false;
            _badgeInfoRoutine = null;
            TryShowNextBadgeInfo();
        }

        private static TMP_Text FindBadgeInfoDescription(Transform root)
        {
            if (root == null) return null;

            if (root.name == "Description")
            {
                TMP_Text rootText = root.GetComponent<TMP_Text>();
                if (rootText != null) return rootText;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                TMP_Text childText = FindBadgeInfoDescription(root.GetChild(i));
                if (childText != null) return childText;
            }

            return null;
        }
    }
}
