using System;
using Data.StaticData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIScripts.MainMenu.Shop
{
    public class ShopItemView : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text priceText;
        [SerializeField] private GameObject coinImage;
        [SerializeField] private Button buyButton;

        private Action _onBuyClicked;

        public void Setup(AccessoryDefinition definition, bool isOwned, Action onBuyClicked)
        {
            if (definition == null) return;
            if (iconImage == null || titleText == null || descriptionText == null || priceText == null) return;
            if (buyButton == null) return;

            _onBuyClicked = onBuyClicked;

            iconImage.sprite = definition.Icon;
            titleText.text = definition.Title;
            descriptionText.text = definition.Description;
            priceText.text = isOwned ? "BOUGHT" : definition.Price.ToString();
            coinImage.SetActive(!isOwned);

            buyButton.onClick.RemoveAllListeners();
            buyButton.interactable = !isOwned;

            if (!isOwned)
            {
                buyButton.onClick.AddListener(OnBuyClicked);
            }
        }

        private void OnBuyClicked()
        {
            _onBuyClicked?.Invoke();
        }
    }
}
