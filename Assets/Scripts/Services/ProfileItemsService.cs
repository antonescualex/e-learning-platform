using System.Collections.Generic;
using Data.StaticData.Item;
using Enums;
using Services.Interfaces;
using UnityEngine;

namespace Services
{
    public class ProfileItemsService : IProfileItemsService
    {
        private readonly BadgeCatalog _badgeCatalog;
        private readonly BoosterCatalog _boosterCatalog;
        private readonly RewardCatalog _rewardCatalog;
        private readonly IProfileService _profileService;

        public ProfileItemsService(
            BadgeCatalog badgeCatalog,
            BoosterCatalog boosterCatalog,
            RewardCatalog rewardCatalog,
            IProfileService profileService)
        {
            _badgeCatalog = badgeCatalog;
            _boosterCatalog = boosterCatalog;
            _rewardCatalog = rewardCatalog;
            _profileService = profileService;
        }

        public IReadOnlyList<ProfileItemDefinition> GetAllItems(ProfileItemCategory profileItemCategory)
        {
            var result = new List<ProfileItemDefinition>();
            if (_profileService == null || !_profileService.HasProfile) return result;

            ProfileItemCatalogBase catalog = GetCatalog(profileItemCategory);
            if (catalog == null) return result;

            IReadOnlyList<string> ids = _profileService.ProfileData.GetItemIds(profileItemCategory);

            foreach (string id in ids)
            {
                ProfileItemDefinition definition = catalog.GetById(id);
                if (definition != null)
                {
                    result.Add(definition);
                }
            }

            return result;
        }

        public IReadOnlyList<ProfileItemDefinition> GetTopItems(ProfileItemCategory category, int count = 4)
        {
            var result = new List<ProfileItemDefinition>(count);

            if (_profileService == null || !_profileService.HasProfile) return result;

            ProfileItemCatalogBase catalog = GetCatalog(category);
            if (catalog == null) return result;

            IReadOnlyList<string> ids = _profileService.ProfileData.GetItemIds(category);
            int pageSize = Mathf.Min(count, ids.Count);

            for (int i = 0; i < pageSize; i++)
            {
                ProfileItemDefinition definition = catalog.GetById(ids[i]);
                if (definition != null)
                {
                    result.Add(definition);
                }
            }

            return result;
        }

        private ProfileItemCatalogBase GetCatalog(ProfileItemCategory category)
        {
            switch (category)
            {
                case ProfileItemCategory.Badges:
                    return _badgeCatalog;
                case ProfileItemCategory.Boosters:
                    return _boosterCatalog;
                case ProfileItemCategory.Rewards:
                    return _rewardCatalog;
                default:
                    return null;
            }
        }
    }
}
