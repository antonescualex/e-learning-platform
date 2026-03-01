using System.Collections;
using Enums;
using Services;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace UIScripts.MainMenu.Profile
{
    public class ProfilePopup : MonoBehaviour
    {
        [SerializeField] private TMP_InputField playerNameInput;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text coinsText;
        [SerializeField] private Button editNameButton;
        [SerializeField] private TMP_Dropdown categoryDropwdown;
        [SerializeField] private ProfileItemSlotView[] itemSlots = new ProfileItemSlotView[4];

        private ProfileMenuController _menuController;
        private IProfileService _profileService;
        private IProfileItemsService _itemsService;

        private bool _isEditingName;
        private bool _ignoreEvents;
        
        public void Init(ProfileMenuController menuController, IProfileService profileService)
        {
            _menuController = menuController;
            _profileService = profileService;
            _itemsService = App.Instance.ProfileItemsService;
            
            if (categoryDropwdown != null)
            {
                categoryDropwdown.onValueChanged.AddListener(OnDropdownChanged);
            }
            
            _profileService.ProfileChanged += OnProfileChanged;
            StartCoroutine(RefreshNextFrame());
            
            
            SetNameEditing(false);
            if (editNameButton != null)
            {
                editNameButton.onClick.AddListener(OnEditNamePressed);
            }
            
            playerNameInput.onEndEdit.AddListener(OnNameEndEdit);
            playerNameInput.onSelect.AddListener(_ => OnNameSelected());
        }
        

        private void OnDestroy()
        {
            if(_profileService != null) _profileService.ProfileChanged -= OnProfileChanged;
        }

        private IEnumerator RefreshNextFrame()
        {
            yield return null;
            
            if (_profileService != null && _profileService.HasProfile)
            {
                OnProfileChanged(_profileService.ProfileData);
            }
            
            // OnDropdownChanged(categoryDropwdown.value);
        }

        private void OnProfileChanged(ProfileData profileData)
        {
            if (profileData == null) return;

            if (playerNameInput != null) playerNameInput.text = profileData.PlayerName;
            if (levelText != null) levelText.text = "Level " + profileData.Level;
            if (coinsText != null) coinsText.text = profileData.Coins.ToString();
        }

        public void OnClosePressed()
        {
            _menuController.CloseProfile();
        }

        private void SetNameEditing(bool enabled)
        {
            _isEditingName = enabled;

            playerNameInput.readOnly = !enabled;
            if(!enabled) playerNameInput.DeactivateInputField();
        }

        private void OnEditNamePressed()
        {
            SetNameEditing(true);
            
            playerNameInput.ActivateInputField();
            playerNameInput.caretPosition = playerNameInput.text.Length;
        }

        private void OnNameSelected()
        {
            if(!_isEditingName) playerNameInput.DeactivateInputField();
        }

        private void OnNameEndEdit(string value)
        {
            if (_ignoreEvents) return;

            value = value?.Trim();
            if (string.IsNullOrEmpty(value))
            {
                OnProfileChanged(_profileService.ProfileData);
                SetNameEditing(false);
                return;
            }

            _profileService.SetPlayerName(value);
            SetNameEditing(false);
        }

        private void OnDropdownChanged(int index)
        {
            ForceReloadItemSlots();
            RefreshItems();
        }

        private ProfileItemCateogory GetSelectedCategory()
        {
            return (ProfileItemCateogory)categoryDropwdown.value;
        }

        private void RefreshItems()
        {
            if (_profileService == null || !_profileService.HasProfile) return;
            if (_itemsService == null) return;

            var profile = _profileService.ProfileData;
            var category = GetSelectedCategory();
            var definitions = _itemsService.GetTopItems(profile, category, 4);

            for (int i = 0; i < itemSlots.Length; i++)
            {
                if (itemSlots[i] == null) continue;

                if (i < definitions.Count)
                {
                    itemSlots[i].Bind(definitions[i]);
                }
                else
                {
                    itemSlots[i].Hide();
                }
            }
            Debug.Log("RefreshItems()");
        }

        private void ForceReloadItemSlots()
        {
            for (int i = 0; i < itemSlots.Length; i++)
            {
                if(itemSlots[i] == null) continue;
                itemSlots[i].gameObject.SetActive(false);
                itemSlots[i].gameObject.SetActive(true);
            }
            Debug.Log("ForceReloadSlots()");
        }
    }
}