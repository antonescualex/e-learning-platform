using System.Collections.Generic;
using Data.StaticData;
using Data.StaticData.Item;
using Enums;
using Services.Interfaces;
using UnityEngine;

namespace Services
{
    public class ProfileItemsService : IProfileItemsService
    {
        private readonly ItemCatalog _catalog;
        private readonly IProfileService _profileService;

        public ProfileItemsService(ItemCatalog catalog, IProfileService profileService)
        {
            _catalog = catalog;
            _profileService = profileService;
        }

        public IReadOnlyList<ItemDefinition> GetAllItems(ProfileItemCateogory profileItemCateogory)
        {
            if (_profileService == null || !_profileService.HasProfile) return null;
            if (_catalog == null) return null;

            var result = new List<ItemDefinition>();
            var ids = _profileService.ProfileData.GetItemIds(profileItemCateogory);

            foreach (var id in ids)
            {
                var definition = _catalog.GetById(id);
                if (definition != null)
                {
                    result.Add(definition);
                }
            }

            return result;
        }

        public IReadOnlyList<ItemDefinition> GetTopItems(ProfileItemCateogory cateogory,
            int count = 4)
        {
            var result = new List<ItemDefinition>(count);

            if (_profileService == null || !_profileService.HasProfile) return result;
            if (_catalog == null) return result;

            var ids = _profileService.ProfileData.GetItemIds(cateogory);
            int pageSize = Mathf.Min(count, ids.Count);

            for (int i = 0; i < pageSize; i++)
            {
                var definition = _catalog.GetById(ids[i]);
                if (definition != null)
                {
                    result.Add(definition);
                }
            }

            return result;
        }
    }
}