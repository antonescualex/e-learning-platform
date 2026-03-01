using System.Collections.Generic;
using Data.StaticData;
using Enums;
using UnityEngine;

namespace Services
{
    public class ProfileItemsService : IProfileItemsService
    {
        private readonly ItemCatalogScriptableObject _catalog;

        public ProfileItemsService(ItemCatalogScriptableObject catalog)
        {
            _catalog = catalog;
        }

        public List<ItemDefinition> GetTopItems(ProfileData profile, ProfileItemCateogory cateogory,
            int count = 4)
        {
            var result = new List<ItemDefinition>(count);
            
            if (profile == null) return result;
            if (_catalog == null) return result;

            var ids = profile.GetItemIds(cateogory);
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