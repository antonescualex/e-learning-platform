using System.Collections.Generic;
using Data.StaticData;
using Data.StaticData.Shop;
using Enums;

namespace Services.Interfaces
{
    public enum ShopPurchaseStatus
    {
        Success,
        ProfileNotLoaded,
        AccessoryNotFound,
        InvalidPrice,
        AlreadyOwned,
        InvalidPurchase,
        NotEnoughCoins
    }

    public interface IShopService
    {
        IReadOnlyList<ShopItemDefinition> GetItems();
        bool IsOwned(string accessoryId);
        ShopPurchaseStatus TryBuy(string accessoryId);
    }
}