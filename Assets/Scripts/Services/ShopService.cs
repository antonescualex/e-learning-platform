using System.Collections.Generic;
using Data.StaticData;
using Data.StaticData.Accessory;

namespace Services
{
    public class ShopService : IShopService
    {
        private readonly AccessoryCatalog _catalog;
        private readonly IProfileService _profileService;

        public ShopService(AccessoryCatalog catalog, IProfileService profileService)
        {
            _catalog = catalog;
            _profileService = profileService;
        }

        public IReadOnlyList<AccessoryDefinition> GetItems()
        {
            if (_catalog == null) return new List<AccessoryDefinition>();

            return _catalog.AccessoryDefinitions;
        }

        public bool IsOwned(string accessoryId)
        {
            if (_profileService == null) return false;
            return _profileService.HasAccessory(accessoryId);
        }

        public ShopPurchaseStatus TryBuy(string accessoryId)
        {
            if (_profileService == null || !_profileService.HasProfile) return ShopPurchaseStatus.ProfileNotLoaded;
            if (_catalog == null) return ShopPurchaseStatus.AccessoryNotFound;

            var definition = _catalog.GetById(accessoryId);
            if (definition == null) return ShopPurchaseStatus.AccessoryNotFound;
            if (_profileService.HasAccessory(accessoryId)) return ShopPurchaseStatus.AlreadyOwned;
            if (!_profileService.TrySpendCoins(definition.Price)) return ShopPurchaseStatus.NotEnoughCoins;
            if (!_profileService.TryAddAccessory(accessoryId))
            {
                _profileService.AddCoins(definition.Price);
                return ShopPurchaseStatus.AlreadyOwned;
            }
            return ShopPurchaseStatus.Success;
        }
    }
}