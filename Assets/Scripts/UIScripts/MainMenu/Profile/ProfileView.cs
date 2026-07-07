using System.Collections;
using System.Collections.Generic;
using Auth;
using Clients;
using Clients.Interfaces;
using Data;
using Data.StaticData.Abstractions;
using Data.StaticData.Badge;
using Data.StaticData.Booster;
using Data.StaticData.Item;
using Enums;
using Services.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIScripts.MainMenu.Profile
{
    public class ProfileView : MonoBehaviour
    {
        [Header("Texts")]
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text coinsText;
        [SerializeField] private TMP_Text createdAtText;
        [SerializeField] private TMP_Text badgesText;
        [SerializeField] private TMP_Text boostersText;
        [SerializeField] private TMP_Text itemsText;
        [SerializeField] private TMP_Text lessonsCompletedText;
        [SerializeField] private TMP_Text lessonsFailedText;

        [Header("Name")]
        [SerializeField] private TMP_InputField playerNameInput;
        [SerializeField] private Button editNameButton;

        [Header("Items")]
        [SerializeField] private TMP_Dropdown categoryDropwdown;
        [SerializeField] private Transform itemsContainer;
        [SerializeField] private ProfileItemSlotView itemPrefab;

        [Header("Item Popups")]
        [SerializeField] private Transform itemPopupParent;
        [SerializeField] private GameObject badgePopupPrefab;
        [SerializeField] private GameObject boosterPopupPrefab;
        [SerializeField] private GameObject boosterDeniedPrefab;

        private ProfileMenuController _menuController;
        private IProfileService _profileService;
        private IBadgeService _badgeService;
        private IBoosterService _boosterService;
        private IProfileClient _profileClient;

        private readonly List<ProfileItemSlotView> _itemList = new List<ProfileItemSlotView>();

        private bool _isUpdatingName;
        private bool _isEditingName;
        private bool _ignoreEvents;
        private bool _popEnabled;
        private GameObject _currentItemPopup;

        public void Init(ProfileMenuController menuController, IProfileService profileService, IBadgeService badgeService, IBoosterService boosterService, IProfileClient profileClient)
        {
            _menuController = menuController;
            _profileService = profileService;
            _badgeService = badgeService;
            _boosterService = boosterService;
            _profileClient = profileClient;

            if (categoryDropwdown != null)
            {
                categoryDropwdown.onValueChanged.AddListener(OnDropdownChanged);
            }

            if (editNameButton != null)
            {
                editNameButton.onClick.AddListener(OnEditNamePressed);
            }

            if (playerNameInput != null)
            {
                playerNameInput.onEndEdit.AddListener(OnNameEndEdit);
                playerNameInput.onSelect.AddListener(OnNameSelected);
            }

            if (_profileService != null)
            {
                _profileService.ProfileChanged += OnProfileChanged;
            }

            SetNameEditing(false);
            StartCoroutine(RefreshNextFrame());
        }


        private void OnDestroy()
        {
            if (categoryDropwdown != null)
            {
                categoryDropwdown.onValueChanged.RemoveListener(OnDropdownChanged);
            }

            if (editNameButton != null)
            {
                editNameButton.onClick.RemoveListener(OnEditNamePressed);
            }

            if (playerNameInput != null)
            {
                playerNameInput.onEndEdit.RemoveListener(OnNameEndEdit);
                playerNameInput.onSelect.RemoveListener(OnNameSelected);
            }

            if (_profileService != null)
            {
                _profileService.ProfileChanged -= OnProfileChanged;
            }

            DestroyCurrentItemPopup();
        }

        private IEnumerator RefreshNextFrame()
        {
            yield return null;

            if (_profileService != null && _profileService.HasProfile)
            {
                OnProfileChanged(_profileService.ProfileData);
            }

            RefreshItems();
        }

        private void OnProfileChanged(ProfileData profileData)
        {
            if (profileData == null) return;

            if (playerNameInput != null && !_isEditingName)
            {
                _ignoreEvents = true;
                playerNameInput.SetTextWithoutNotify(profileData.PlayerName);
                _ignoreEvents = false;
            }

            if (levelText != null) levelText.text = "Level " + profileData.Level;
            if (coinsText != null) coinsText.text = profileData.Coins.ToString();
            if (createdAtText != null) createdAtText.text = "Member since: " + profileData.CreatedAt;
            if (badgesText != null) badgesText.text = profileData.BadgeItemIds.Count.ToString();
            if (boostersText != null) boostersText.text = profileData.BoosterItemIds.Count.ToString();
            if (itemsText != null) itemsText.text =
                (profileData.BackgroundItemIds.Count + profileData.AvatarItemIds.Count).ToString();
            if (lessonsCompletedText != null) lessonsCompletedText.text = profileData.CompletedLessonsCount.ToString();
            if (lessonsFailedText != null) lessonsFailedText.text = profileData.IncompleteLessonsCount.ToString();
            RefreshItems();
        }

        public void OnClosePressed()
        {
            DestroyCurrentItemPopup();
            _menuController.CloseProfile();
        }

        private void SetNameEditing(bool enabled)
        {
            _isEditingName = enabled;

            if (playerNameInput == null) return;
            playerNameInput.readOnly = !enabled;
            if (!enabled) playerNameInput.DeactivateInputField();
        }

        private void OnEditNamePressed()
        {
            SetNameEditing(true);
            playerNameInput.ActivateInputField();
            playerNameInput.caretPosition = playerNameInput.text.Length;
        }

        private void OnNameSelected(string _)
        {
            if (!_isEditingName && playerNameInput != null)
            {
                playerNameInput.DeactivateInputField();
            }
        }

        private void OnNameEndEdit(string value)
        {
            if (_ignoreEvents) return;
            if (_profileService == null || _profileClient == null) return;
            if (_isUpdatingName) return;

            string newName = value?.Trim();
            if (string.IsNullOrWhiteSpace(newName))
            {
                RestoreCurrentProfileName();
                SetNameEditing(false);
                return;
            }

            if (_profileService.HasProfile && newName == _profileService.ProfileData.PlayerName)
            {
                SetNameEditing(false);
                return;
            }

            StartCoroutine(UpdatePlayerName(newName));
        }
        
        private IEnumerator UpdatePlayerName(string newName)
        {
            _isUpdatingName = true;

            ProfileDto profileDto = null;
            string error = null;

            yield return _profileClient.UpdatePlayerName(
                newName,
                dto => profileDto = dto,
                message => error = message);

            _isUpdatingName = false;

            if (!string.IsNullOrWhiteSpace(error) || profileDto == null)
            {
                Debug.LogWarning("Update player name failed:\n" + error);
                RestoreCurrentProfileName();
                SetNameEditing(false);
                yield break;
            }

            SetNameEditing(false);
            _profileService.SetLoadedProfile(ProfileMapper.ToProfileData(profileDto));
        }

        private void RestoreCurrentProfileName()
        {
            if (playerNameInput == null) return;
            if (_profileService == null || !_profileService.HasProfile) return;

            _ignoreEvents = true;
            playerNameInput.SetTextWithoutNotify(_profileService.ProfileData.PlayerName);
            _ignoreEvents = false;
        }

        private void OnDropdownChanged(int index)
        {
            if (!_popEnabled) _popEnabled = true;

            RefreshItems();
        }

        private ProfileItemCategory GetSelectedCategory()
        {
            if (categoryDropwdown == null) return ProfileItemCategory.Badges;
            return (ProfileItemCategory)categoryDropwdown.value;
        }

        private void RefreshItems()
        {
            ClearItems();

            if (_profileService == null || !_profileService.HasProfile) return;
            if (itemsContainer == null || itemPrefab == null) return;
            
            IReadOnlyList<ProfileItemDefinition> definitions;
            switch (GetSelectedCategory())
            {
                case ProfileItemCategory.Badges:
                    definitions = _badgeService != null ? _badgeService.GetBadges() : new List<ProfileItemDefinition>();
                    break;
                case ProfileItemCategory.Boosters:
                    definitions = _boosterService != null ? _boosterService.GetBoosters() : new List<ProfileItemDefinition>();
                    break;
                default:
                    definitions = new List<ProfileItemDefinition>();
                    break;
            }

            for (int i = 0; i < definitions.Count; i++)
            {
                ProfileItemDefinition definition = definitions[i];
                var itemView = Instantiate(itemPrefab, itemsContainer, false);
                itemView.gameObject.name = $"ProfileItem_{definition.Id}";

                if (_popEnabled) itemView.ShowWithPop();
                else itemView.ShowWithoutPop();

                itemView.Bind(definition);
                itemView.SetClickHandler(() => HandleItemClicked(definition));
            }
        }

        private void ClearItems()
        {
            if (itemsContainer == null) return;

            for (int i = itemsContainer.childCount - 1; i >= 0; i--)
            {
                var child = itemsContainer.GetChild(i).gameObject;
                child.SetActive(false);
                Destroy(child);
            }
        }

        private void HandleItemClicked(ProfileItemDefinition itemDefinition)
        {
            if (itemDefinition == null) return;

            switch (itemDefinition.Category)
            {
                case ProfileItemCategory.Badges:
                    HandleBadgeClicked(itemDefinition as BadgeDefinition);
                    break;
                case ProfileItemCategory.Boosters:
                    HandleBoosterClicked(itemDefinition as BoosterDefinition);
                    break;
            }
        }

        private void HandleBadgeClicked(BadgeDefinition badgeDefinition)
        {
            if (badgeDefinition == null) return;
            if (badgePopupPrefab == null || itemPopupParent == null) return;

            DestroyCurrentItemPopup();

            _currentItemPopup = Instantiate(badgePopupPrefab, itemPopupParent, false);
            _currentItemPopup.transform.SetAsLastSibling();

            BadgePopupView popupView = _currentItemPopup.GetComponent<BadgePopupView>();
            popupView?.Bind(badgeDefinition);

            ProfileItemPopup popup = _currentItemPopup.GetComponent<ProfileItemPopup>();
            popup?.SetPopupCanvas(itemPopupParent);
            popup?.Open();
        }

        private void HandleBoosterClicked(BoosterDefinition boosterDefinition)
        {
            if (boosterDefinition == null) return;
            if (boosterPopupPrefab == null || itemPopupParent == null || boosterDeniedPrefab == null) return;

            bool isBoosterActive = _boosterService.IsBoosterActive();
            
            GameObject popupPrefab = isBoosterActive ? boosterDeniedPrefab : boosterPopupPrefab;
            if (popupPrefab == null) return;
            
            DestroyCurrentItemPopup();

            _currentItemPopup = Instantiate(popupPrefab, itemPopupParent, false);
            _currentItemPopup.transform.SetAsLastSibling();

            if (!isBoosterActive)
            {
                BoosterPopupView boosterPopupView = _currentItemPopup.GetComponent<BoosterPopupView>();
                boosterPopupView?.Bind(boosterDefinition);
            }
            
            ProfileItemPopup popup = _currentItemPopup.GetComponent<ProfileItemPopup>();
            popup?.SetPopupCanvas(itemPopupParent);
            popup?.Open();
        }

        private void DestroyCurrentItemPopup()
        {
            if (_currentItemPopup == null) return;

            Destroy(_currentItemPopup);
            _currentItemPopup = null;
        }

    }
}
