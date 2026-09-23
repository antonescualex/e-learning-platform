using System;
using System.Collections;
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
        NotEnoughCoins,
        RequestFailed
    }

    public interface IShopService
    {
        IReadOnlyList<ShopItemDefinition> GetItems();
        bool IsOwned(string accessoryId);
        IEnumerator TryBuy(
            string itemId,
            Action<ShopPurchaseStatus> onComplete,
            Action<string> onError);
    }
}