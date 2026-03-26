using Data.StaticData;
using Services;
using Services.Interfaces;
using UnityEngine;

namespace UIScripts.MainMenu.Shop
{
    public class ShopPopup : MonoBehaviour
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

                bool isOwned = _shopService.IsOwned(definition.Id);
                view.Setup(definition, isOwned, () => OnBuyPressed(definition.Id));
            }
        }

        private void OnBuyPressed(string accessoryId)
        {
            if (_shopService == null) return;

            var result = _shopService.TryBuy(accessoryId);
            if (result == ShopPurchaseStatus.Success)
            {
                PopulateShop();
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
