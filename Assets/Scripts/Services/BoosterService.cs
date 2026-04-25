using System;
using System.Collections.Generic;
using Data.StaticData.Item;
using Enums;
using Services.Interfaces;

namespace Services
{
    public class BoosterService : IBoosterService
    {
        private const int DefaultMultiplier = 1;
        private const int ActiveBoosterMultiplier = 2;
        
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

        public bool TryActivateBooster(string boosterItemId)
        {
            if(IsBoosterActive()) return false;
            
            BoosterDefinition booster = _boosterCatalog.GetBoosterById(boosterItemId);
            if (booster == null) return false;

            bool removed = _profileService.ProfileData.TryRemoveBoosterItem(boosterItemId);
            if (!removed) return false;

            _profileService.ProfileData.ActivateBooster(
                booster.BoosterType,
                booster.DurationSeconds);

            _profileService.SaveProfile();
            return true;
        }

        public int GetRewardMultiplier(BoosterType boosterType)
        {
            return _profileService.ProfileData.IsBoosterActive(boosterType) ? 2 : 1;
        }

        public TimeSpan GetRemainingTime(BoosterType boosterType)
        {
            return _profileService.ProfileData.GetBoosterRemainingTime(boosterType);
        }

        public bool IsBoosterActive(BoosterType boosterType)
        {
            return _profileService.ProfileData.IsBoosterActive(boosterType);
        }

        public bool IsBoosterActive()
        {
            return _profileService.ProfileData.IsBoosterActive(BoosterType.DoubleXP) ||
                   _profileService.ProfileData.IsBoosterActive(BoosterType.DoubleCoins);
        }
    }
}