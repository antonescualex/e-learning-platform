using System;
using System.Collections;
using System.Collections.Generic;
using Clients;
using Clients.Interfaces;
using Data.StaticData.Shop;
using Dto.Profile;
using Enums;
using Services.Interfaces;

namespace Services
{
    public class ShopService : IShopService
    {
        private readonly ShopCatalog _catalog;
        private readonly IProfileService _profileService;
        private readonly IBadgeService _badgeService;
        private readonly IShopClient _shopClient;

        public ShopService(ShopCatalog catalog, IProfileService profileService, IBadgeService badgeService, IShopClient shopClient)
        {
            _catalog = catalog;
            _profileService = profileService;
            _badgeService = badgeService;
            _shopClient = shopClient;
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

        public IEnumerator TryBuy(
            string itemId,
            Action<ShopPurchaseStatus> onComplete,
            Action<string> onError)
        {
            if (_profileService == null || !_profileService.HasProfile)
            {
                onComplete?.Invoke(ShopPurchaseStatus.ProfileNotLoaded);
                yield break;
            }

            if (_catalog == null)
            {
                onComplete?.Invoke(ShopPurchaseStatus.AccessoryNotFound);
                yield break;
            }

            var definition = _catalog.GetById(itemId);
            if (definition == null)
            {
                onComplete?.Invoke(ShopPurchaseStatus.AccessoryNotFound);
                yield break;
            }

            if (definition.Category == ShopItemCategory.Avatar && _profileService.HasAvatar(itemId))
            {
                onComplete?.Invoke(ShopPurchaseStatus.AlreadyOwned);
                yield break;
            }

            if (definition.Category == ShopItemCategory.Background && _profileService.HasBackground(itemId))
            {
                onComplete?.Invoke(ShopPurchaseStatus.AlreadyOwned);
                yield break;
            }

            ProfileAwardResponse response = null;
            string errorMessage = null;

            switch (definition.Category)
            {
                case ShopItemCategory.Avatar:
                    yield return _shopClient.PurchaseAvatar(
                        itemId,
                        result => response = result,
                        error => errorMessage = error);
                    break;

                case ShopItemCategory.Background:
                    yield return _shopClient.PurchaseBackground(
                        itemId,
                        result => response = result,
                        error => errorMessage = error);
                    break;

                case ShopItemCategory.Booster:
                    yield return _shopClient.PurchaseBooster(
                        itemId,
                        result => response = result,
                        error => errorMessage = error);
                    break;

                default:
                    onComplete?.Invoke(ShopPurchaseStatus.InvalidPurchase);
                    yield break;
            }

            if (!string.IsNullOrWhiteSpace(errorMessage) || response == null || response.Profile == null)
            {
                onError?.Invoke(errorMessage);
                onComplete?.Invoke(ShopPurchaseStatus.RequestFailed);
                yield break;
            }

            _profileService.SetLoadedProfile(ProfileMapper.ToProfileData(response.Profile));
            _badgeService?.EnqueueAwardedBadges(response.AwardedBadgeIds);
            onComplete?.Invoke(ShopPurchaseStatus.Success);
        }
    }
}
