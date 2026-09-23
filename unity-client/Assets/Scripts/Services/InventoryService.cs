using System.Collections.Generic;
using Data.StaticData.Background;
using Data.StaticData.Item;
using Services.Interfaces;

namespace Services
{
    public class InventoryService : IInventoryService
    {
        private readonly BackgroundCatalog _backgroundCatalog;
        private readonly IProfileService _profileService;

        public InventoryService(BackgroundCatalog backgroundCatalog, IProfileService profileService)
        {
            _backgroundCatalog = backgroundCatalog;
            _profileService = profileService;
        }

        public IReadOnlyList<InventoryItem> GetItems()
        {
            var result = new List<InventoryItem>();

            if (_profileService == null || !_profileService.HasProfile) return result;
            if (_backgroundCatalog == null) return result;

            var itemIds = _profileService.ProfileData.BackgroundItemIds;
            if (itemIds == null) return result;

            for (int i = 0; i < itemIds.Count; i++)
            {
                var itemId = itemIds[i];
                if (string.IsNullOrEmpty(itemId)) continue;

                var definition = _backgroundCatalog.GetBackgroundDefinition(itemId);
                if (definition == null) continue;

                result.Add(new InventoryItem(definition.id, definition.title, definition.sprite));
            }

            return result;
        }
    }

}