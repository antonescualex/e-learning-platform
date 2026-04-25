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
        [SerializeField] private TMP_Text equipButtonText;
        [SerializeField] private Image equipButtonImage;
        [SerializeField] private Sprite equippedSprite;
        [SerializeField] private Sprite defaultSprite;

        private InventoryItem _inventoryItem;
        
        public void Bind(InventoryItem inventoryItem, bool isEquipped)
        {
            if (iconImage == null || titleText == null || equipButton == null) return;

            _inventoryItem = inventoryItem;
            
            iconImage.sprite = inventoryItem.Icon;
            titleText.text = inventoryItem.Title;
            
            equipButtonText.text = isEquipped ? "EQUIPPED" : "EQUIP";
            equipButton.interactable = !isEquipped;
            equipButtonImage.sprite = isEquipped ? equippedSprite : defaultSprite;
            
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