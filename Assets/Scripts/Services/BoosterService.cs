using System.Collections.Generic;
using Data.StaticData.Item;
using Enums;
using Services.Interfaces;

namespace Services
{
    public class BoosterService : IBoosterService
    {
        private BoosterCatalog _boosterCatalog;
        private IProfileService _profileService;

        public BoosterService(BoosterCatalog boosterCatalog, IProfileService profileService)
        {
            _boosterCatalog = boosterCatalog;
            _profileService = profileService;
        }
        
        public IReadOnlyList<ProfileItemDefinition> GetBoosters()
        {
            var result = new List<ProfileItemDefinition>();
            if (_profileService == null || !_profileService.HasProfile) return result;
            if (_boosterCatalog == null) return result;

            IReadOnlyList<string> ids = _profileService.ProfileData.GetItemIds(ProfileItemCategory.Boosters);

            foreach (string id in ids)
            {
                ProfileItemDefinition definition = _boosterCatalog.GetById(id);
                if (definition != null)
                {
                    result.Add(definition);
                }
            }

            return result;
        }
    }
}