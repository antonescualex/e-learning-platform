using System.Collections.Generic;
using Data.StaticData;
using Data.StaticData.Accessory;

namespace Services
{
    public enum ShopPurchaseStatus
    {
        Success,
        ProfileNotLoaded,
        AccessoryNotFound,
        InvalidPrice,
        AlreadyOwned,
        NotEnoughCoins
    }

    public interface IShopService
    {
        IReadOnlyList<AccessoryDefinition> GetItems();
        bool IsOwned(string accessoryId);
        ShopPurchaseStatus TryBuy(string accessoryId);
    }
}