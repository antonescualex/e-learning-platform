using System;
using System.Collections;
using System.Collections.Generic;
using Enums;
using Services;
using Services.Interfaces;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace UIScripts.MainMenu.Profile
{
    public class ProfilePopup : MonoBehaviour
    {
        [Header("Texts")]
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text coinsText;
        [SerializeField] private TMP_Text createdAtText;
        [SerializeField] private TMP_Text badgesText;
        [SerializeField] private TMP_Text boostersText;
        [SerializeField] private TMP_Text rewardsText;
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

        // [Header("Page Buttons")] 
        // [SerializeField]private Button leftArrowButton;
        // [SerializeField]private Button rightArrowButton;

        private ProfileMenuController _menuController;
        private IProfileService _profileService;
        private IProfileItemsService _itemsService;

        private readonly List<ProfileItemSlotView> _itemList = new List<ProfileItemSlotView>();

        private bool _isEditingName;
        private bool _ignoreEvents;
        private bool _popEnabled;

        // private const int PageSize = 4;
        // private int _currentPage = 0;

        public void Init(ProfileMenuController menuController, IProfileService profileService, IProfileItemsService itemsService)
        {
            _menuController = menuController;
            _profileService = profileService;
            _itemsService = itemsService;

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
            if (rewardsText != null) rewardsText.text = profileData.RewardItemIds.Count.ToString();
            if (itemsText != null) itemsText.text = profileData.AccessoryItemIds.Count.ToString();
            if (lessonsCompletedText != null) lessonsCompletedText.text = profileData.CompletedLessonsCount.ToString();
            if (lessonsFailedText != null) lessonsFailedText.text = profileData.IncompleteLessonsCount.ToString();
            RefreshItems();
        }

        public void OnClosePressed()
        {
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
            if (_profileService == null) return;

            _profileService.SetPlayerName(value);

            if (playerNameInput != null && _profileService.HasProfile)
            {
                _ignoreEvents = true;
                playerNameInput.SetTextWithoutNotify(_profileService.ProfileData.PlayerName);
                _ignoreEvents = false;
            }

            SetNameEditing(false);
        }

        private void OnDropdownChanged(int index)
        {
            if (!_popEnabled) _popEnabled = true;

            RefreshItems();
        }

        private ProfileItemCateogory GetSelectedCategory()
        {
            if (categoryDropwdown == null) return ProfileItemCateogory.Badges;
            return (ProfileItemCateogory)categoryDropwdown.value;
        }

        private void RefreshItems()
        {
            ClearItems();

            if (_profileService == null || !_profileService.HasProfile) return;
            if (_itemsService == null) return;
            if (itemsContainer == null || itemPrefab == null) return;

            var category = GetSelectedCategory();
            var definitions = _itemsService.GetAllItems(category);

            for (int i = 0; i < definitions.Count; i++)
            {
                var itemView = Instantiate(itemPrefab, itemsContainer, false);
                itemView.gameObject.name = $"ProfileItem_{definitions[i].Id}";

                if (_popEnabled) itemView.ShowWithPop();
                else itemView.ShowWithoutPop();

                itemView.Bind(definitions[i]);
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

    }
}
