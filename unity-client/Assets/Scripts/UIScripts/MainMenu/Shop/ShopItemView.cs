using System;
using Data.StaticData;
using Data.StaticData.Shop;
using Enums;
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
        [SerializeField] private Sprite ownedButtonSprite;

        private Action _onBuyClicked;
        private ShopItemDefinition _definition;
        private bool _isOwned;
        private Image _buyButtonBackgroundImage;
        private Sprite _availableButtonSprite;

        public void Setup(ShopItemDefinition definition, bool isOwned, Action onBuyClicked)
        {
            if (definition == null) return;
            if (iconImage == null || titleText == null || descriptionText == null || priceText == null) return;
            if (buyButton == null) return;

            _definition = definition;
            _isOwned = isOwned;
            _onBuyClicked = onBuyClicked;

            iconImage.sprite = definition.Icon;
            if (definition.Category == ShopItemCategory.Background)
            {
                iconImage.rectTransform.sizeDelta = new Vector2(80, 80);
            }
            
            titleText.text = definition.Title;
            descriptionText.text = definition.Description;
            CacheButtonVisualState();
            RefreshOwnedState();
        }

        private void OnBuyClicked()
        {
            _onBuyClicked?.Invoke();
        }

        public void MarkAsOwned()
        {
            _isOwned = true;
            _onBuyClicked = null;
            RefreshOwnedState();
        }

        private void RefreshOwnedState()
        {
            if (_definition == null || priceText == null || buyButton == null) return;

            priceText.text = _isOwned ? "BOUGHT" : _definition.Price.ToString();
            priceText.horizontalAlignment = _isOwned ? HorizontalAlignmentOptions.Left : HorizontalAlignmentOptions.Center;

            if (coinImage != null)
            {
                coinImage.SetActive(!_isOwned);
            }

            buyButton.onClick.RemoveAllListeners();
            buyButton.interactable = !_isOwned;
            ApplyButtonVisualState();

            if (!_isOwned)
            {
                buyButton.onClick.AddListener(OnBuyClicked);
            }
        }

        private void ApplyButtonVisualState()
        {
            if (_buyButtonBackgroundImage == null) return;

            Sprite targetSprite = _isOwned ? ownedButtonSprite : _availableButtonSprite;
            if (targetSprite == null) return;

            _buyButtonBackgroundImage.sprite = targetSprite;
        }

        private void CacheButtonVisualState()
        {
            if (_buyButtonBackgroundImage == null)
            {
                _buyButtonBackgroundImage = ResolveButtonBackgroundImage();
            }

            if (_availableButtonSprite == null && _buyButtonBackgroundImage != null)
            {
                _availableButtonSprite = _buyButtonBackgroundImage.sprite;
            }
        }

        private Image ResolveButtonBackgroundImage()
        {
            if (buyButton == null) return null;

            if (buyButton.targetGraphic is Image targetGraphicImage)
            {
                return targetGraphicImage;
            }

            Transform backgroundTransform = buyButton.transform.Find("Button");
            if (backgroundTransform != null)
            {
                return backgroundTransform.GetComponent<Image>();
            }

            return buyButton.GetComponent<Image>();
        }
    }
}
