using System.Collections.Generic;
using Data.StaticData.Accessory;
using Data.StaticData.Item;
using Services.Interfaces;

namespace Services
{
    public class InventoryService : IInventoryService
    {
        private readonly AccessoryCatalog _catalog;
        private readonly IProfileService _profileService;

        public InventoryService(AccessoryCatalog catalog, IProfileService profileService)
        {
            _catalog = catalog;
            _profileService = profileService;
        }

        public IReadOnlyList<InventoryItem> GetItems()
        {
            var result = new List<InventoryItem>();

            if (_profileService == null || !_profileService.HasProfile) return result;
            if (_catalog == null) return result;

            var itemIds = _profileService.ProfileData.AccessoryItemIds;
            if (itemIds == null) return result;

            for (int i = 0; i < itemIds.Count; i++)
            {
                var itemId = itemIds[i];
                if (string.IsNullOrEmpty(itemId)) continue;

                var definition = _catalog.GetById(itemId);
                if (definition == null) continue;

                result.Add(new InventoryItem(definition.Id, definition.Title, definition.Icon));
            }

            return result;
        }
    }

}