using System;
using System.Collections;
using System.Collections.Generic;
using Auth;
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
        private IProfileClient _profileClient;

        public BoosterService(BoosterCatalog boosterCatalog, IProfileService profileService, IProfileClient profileClient)
        {
            _boosterCatalog = boosterCatalog;
            _profileService = profileService;
            _profileClient = profileClient;
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

        public IEnumerator ActivateBooster(
            string boosterItemId,
            Action onSuccess,
            Action<string> onError)
        {
            if (string.IsNullOrWhiteSpace(boosterItemId))
            {
                onError?.Invoke("BoosterItemId is required.");
                yield break;
            }

            if (_profileClient == null || _profileService == null)
            {
                onError?.Invoke("Booster service is not initialized.");
                yield break;
            }

            ProfileDto profileDto = null;
            string error = null;

            yield return _profileClient.ActivateBooster(
                boosterItemId,
                dto => profileDto = dto,
                message => error = message);

            if (!string.IsNullOrWhiteSpace(error) || profileDto == null)
            {
                onError?.Invoke(error);
                yield break;
            }

            _profileService.SetLoadedProfile(ProfileMapper.ToProfileData(profileDto));

            onSuccess?.Invoke();
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