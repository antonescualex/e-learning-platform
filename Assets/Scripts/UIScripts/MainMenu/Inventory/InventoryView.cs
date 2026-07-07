using System.Collections;
using Auth;
using Clients;
using Clients.Interfaces;
using Data.StaticData.Item;
using Services;
using Services.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace UIScripts.MainMenu.Inventory
{
    public class InventoryView : MonoBehaviour
    {
        [SerializeField] private Transform content;
        [SerializeField] private GameObject itemPrefab;

        private InventoryMenuController _menuController;
        private IInventoryService _inventoryService;
        private IProfileService _profileService;
        private IProfileClient _profileClient;
        private bool _isSelectingBackground;

        public void Init(InventoryMenuController menuController, IInventoryService inventoryService, IProfileService profileService, IProfileClient profileClient)
        {
            _menuController = menuController;
            _inventoryService = inventoryService;
            _profileService = profileService;
            _profileClient = profileClient;
            Populate();
        }

        public void Populate()
        {
            if (content == null || itemPrefab == null || _inventoryService == null) return;

            ClearContent();

            var items = _inventoryService.GetItems();
            foreach (var item in items)
            {
                if (item == null) continue;

                var itemGameObject = Instantiate(itemPrefab, content, false);
                var view = itemGameObject.GetComponent<InventoryItemView>();
                view.Bind(item, _profileService.ProfileData.BackgroundId == item.Id);
                view.EquipRequested += OnEquipRequested;
            }
        }
        
        public void OnClosePressed()
        {
            _menuController.CloseInventory();
        }

        private void OnEquipRequested(InventoryItem inventoryItem)
        {
            if (_profileService == null || inventoryItem == null) return;
            
            StartCoroutine(SelectBackground(inventoryItem.Id));
        }
        
        private void ClearContent()
        {
            for (int i = content.childCount - 1; i >= 0; i--)
            {
                Destroy(content.GetChild(i).gameObject);
            }
        }
        
        private IEnumerator SelectBackground(string backgroundId)
        {
            if (_isSelectingBackground) yield break;
            if (_profileClient == null || _profileService == null) yield break;

            _isSelectingBackground = true;

            ProfileDto profileDto = null;
            string error = null;

            yield return _profileClient.SelectBackground(
                backgroundId,
                dto => profileDto = dto,
                message => error = message);

            _isSelectingBackground = false;

            if (!string.IsNullOrWhiteSpace(error) || profileDto == null)
            {
                Debug.LogWarning("Select background failed:\n" + error);
                Populate();
                yield break;
            }

            _profileService.SetLoadedProfile(ProfileMapper.ToProfileData(profileDto));
            Populate();
        }
    }
}