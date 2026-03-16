using Data.StaticData;
using Data.StaticData.Item;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIScripts.MainMenu.Inventory
{
    public class InventoryItemView : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Image shadowImage;
        [SerializeField] private Button equipButton;

        public void Bind(InventoryItem inventoryItem)
        {
            if (iconImage == null || shadowImage == null || equipButton == null) return;

            iconImage.sprite = inventoryItem.Icon;
            shadowImage.sprite = inventoryItem.Icon;

            //TODO: Implementare logica equip;
        }
    }
}