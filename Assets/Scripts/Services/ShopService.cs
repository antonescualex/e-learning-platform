using System.Collections.Generic;
using Data.StaticData;
using Data.StaticData.Shop;
using Enums;
using Services.Interfaces;

namespace Services
{
    public class ShopService : IShopService
    {
        private readonly ShopCatalog _catalog;
        private readonly IProfileService _profileService;
        private readonly IBadgeService _badgeService;

        public ShopService(ShopCatalog catalog, IProfileService profileService, IBadgeService badgeService)
        {
            _catalog = catalog;
            _profileService = profileService;
            _badgeService = badgeService;
        }

        public IReadOnlyList<ShopItemDefinition> GetItems()
        {
            if (_catalog == null) return new List<ShopItemDefinition>();

            return _catalog.ShopItemDefinitions;
        }

        public bool IsOwned(string itemId)
        {
            if (_profileService == null) return false;
            if (_catalog == null) return false;

            var definition = _catalog.GetById(itemId);
            if (definition == null) return false;

            switch (definition.Category)
            {
                case ShopItemCategory.Avatar:
                    return _profileService.HasAvatar(itemId);

                case ShopItemCategory.Background:
                    return _profileService.HasBackground(itemId);

                case ShopItemCategory.Booster:
                    return false;

                default:
                    return false;
            }
        }

        public ShopPurchaseStatus TryBuy(string itemId)
        {
            if (_profileService == null || !_profileService.HasProfile)
                return ShopPurchaseStatus.ProfileNotLoaded;

            if (_catalog == null)
                return ShopPurchaseStatus.AccessoryNotFound;

            var definition = _catalog.GetById(itemId);
            if (definition == null)
                return ShopPurchaseStatus.AccessoryNotFound;

            if (definition.Category == ShopItemCategory.Avatar && _profileService.HasAvatar(itemId))
                return ShopPurchaseStatus.AlreadyOwned;

            if (definition.Category == ShopItemCategory.Background && _profileService.HasBackground(itemId))
                return ShopPurchaseStatus.AlreadyOwned;

            if (!_profileService.TrySpendCoins(definition.Price))
                return ShopPurchaseStatus.NotEnoughCoins;

            bool added;
            if (definition.Category == ShopItemCategory.Avatar)
                added = _profileService.TryAddAvatar(itemId);
            else if (definition.Category == ShopItemCategory.Background)
                added = _profileService.TryAddBackground(itemId);
            else if (definition.Category == ShopItemCategory.Booster)
                added = _profileService.TryAddBoosterItem(itemId);
            else added = false;
            
            if (!added)
            {
                _profileService.AddCoins(definition.Price);
                return ShopPurchaseStatus.AlreadyOwned;
            }

            _profileService.RegisterShopPurchase(definition.Price);
            _badgeService?.HandleShopPurchase();

            return ShopPurchaseStatus.Success;
        }
    }
}
