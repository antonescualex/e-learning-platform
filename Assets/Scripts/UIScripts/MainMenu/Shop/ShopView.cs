using Data.StaticData;
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
                view.Setup(definition, isOwned, () => OnBuyPressed(view, definition.Id));
            }
        }

        private void OnBuyPressed(ShopItemView itemView, string accessoryId)
        {
            if (_shopService == null) return;
            if (itemView == null) return;

            var result = _shopService.TryBuy(accessoryId);
            if (result == ShopPurchaseStatus.Success)
            {
                itemView.MarkAsOwned();
                return;
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
