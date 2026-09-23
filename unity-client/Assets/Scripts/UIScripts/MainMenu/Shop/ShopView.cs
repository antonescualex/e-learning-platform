using System.Collections;
using Data.StaticData;
using Data.StaticData.Shop;
using Enums;
using Services;
using Services.Interfaces;
using UIScripts.Bootstrap;
using UnityEngine;

namespace UIScripts.MainMenu.Shop
{
    public class ShopView : MonoBehaviour
    {
        [SerializeField] private GameObject accessoryPrefab;
        [SerializeField] private Transform content;

        private ShopMenuController _shopMenuController;
        private IShopService _shopService;
        private bool _isBuying;

        public void Init(IShopService shopService, ShopMenuController shopMenuController)
        {
            _shopService = shopService;
            _shopMenuController = shopMenuController;
            PopulateShop();
        }

        public void PopulateShop()
        {
            if (_shopService == null || accessoryPrefab == null || content == null) return;

            ClearContent();

            foreach (var definition in _shopService.GetItems())
            {
                if (definition == null) continue;

                var itemGameObject = Instantiate(accessoryPrefab, content, false);
                var view = itemGameObject.GetComponent<ShopItemView>();
                if (view == null) continue;

                bool isOwned = _shopService.IsOwned(definition.Id);
                view.Setup(definition, isOwned, () => OnBuyPressed(view, definition));
            }
        }

        private void OnBuyPressed(ShopItemView itemView, ShopItemDefinition definition)
        {
            if (_isBuying) return;
            if (_shopService == null || itemView == null || definition == null) return;

            StartCoroutine(BuyItem(itemView, definition));
        }
        
        private IEnumerator BuyItem(ShopItemView itemView, ShopItemDefinition definition)
        {
            _isBuying = true;

            ShopPurchaseStatus result = ShopPurchaseStatus.InvalidPurchase;
            string error = null;

            yield return _shopService.TryBuy(
                definition.Id,
                status => result = status,
                message => error = message);

            _isBuying = false;

            if (result != ShopPurchaseStatus.Success)
            {
                Debug.LogWarning("Shop purchase failed: " + result + "\n" + error);
                yield break;
            }

            if (definition.Category != ShopItemCategory.Booster)
            {
                itemView.MarkAsOwned();
            }
        }

        private void ClearContent()
        {
            for (int i = content.childCount - 1; i >= 0; i--)
            {
                Destroy(content.GetChild(i).gameObject);
            }
        }

        public void CloseShop()
        {
            _shopMenuController.CloseShop();
        }
    }
}
