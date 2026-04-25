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

        public void Init(InventoryMenuController menuController, IInventoryService inventoryService, IProfileService profileService)
        {
            _menuController = menuController;
            _inventoryService = inventoryService;
            _profileService = profileService;
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
            
            _profileService.SetBackground(inventoryItem.Id);
            Populate();
        }
        
        private void ClearContent()
        {
            for (int i = content.childCount - 1; i >= 0; i--)
            {
                Destroy(content.GetChild(i).gameObject);
            }
        }
    }
}