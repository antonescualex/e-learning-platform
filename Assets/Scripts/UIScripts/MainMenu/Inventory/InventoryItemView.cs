using System;
using Data.StaticData.Item;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIScripts.MainMenu.Inventory
{
    public class InventoryItemView : MonoBehaviour
    {
        public event Action<InventoryItem> EquipRequested;
        
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private Button equipButton;

        private InventoryItem _inventoryItem;
        
        public void Bind(InventoryItem inventoryItem)
        {
            if (iconImage == null || titleText == null || equipButton == null) return;

            _inventoryItem = inventoryItem;
            
            iconImage.sprite = inventoryItem.Icon;
            titleText.text = inventoryItem.Title;
            
            equipButton.onClick.RemoveAllListeners();
            equipButton.onClick.AddListener(OnEquipPressed);
        }
        
        private void OnEquipPressed()
        {
            if (_inventoryItem == null) return;

            EquipRequested?.Invoke(_inventoryItem);
        }
    }
}