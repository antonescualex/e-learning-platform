using Services;
using Services.Interfaces;
using UnityEngine;

namespace UIScripts.MainMenu.Inventory
{
    public class InventoryView : MonoBehaviour
    {
        [SerializeField] private Transform content;
        [SerializeField] private GameObject itemPrefab;

        private InventoryMenuController _menuController;
        private IInventoryService _inventoryService;

        public void Init(InventoryMenuController menuController, IInventoryService inventoryService)
        {
            _menuController = menuController;
            _inventoryService = inventoryService;
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
                view.Bind(item);
            }
        }

        private void ClearContent()
        {
            for (int i = content.childCount - 1; i >= 0; i--)
            {
                Destroy(content.GetChild(i).gameObject);
            }
        }

        public void OnClosePressed()
        {
            _menuController.CloseInventory();
        }
    }
}