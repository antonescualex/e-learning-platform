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
            if (_shopService == null) return;
            if (itemView == null) return;
            if (definition == null) return;
            
            var result = _shopService.TryBuy(definition.Id);
            if (result != ShopPurchaseStatus.Success) return;
            
            if(definition.Category != ShopItemCategory.Booster)
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
