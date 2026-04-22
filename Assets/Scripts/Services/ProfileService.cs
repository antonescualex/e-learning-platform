using System;
using Data;
using Repositories;
using Services.Interfaces;

namespace Services
{
    public class ProfileService : IProfileService
    {
        private readonly IRepository<ProfileData> _repository;
        private ProfileData _profileData;

        public event Action<ProfileData> ProfileChanged;
        public ProfileData ProfileData => _profileData;
        public bool HasProfile => _profileData != null;

        public ProfileService(IRepository<ProfileData> repository)
        {
            _repository = repository;
        }

        public bool TryLoadProfile()
        {
            if (_repository.TryLoad(out var loadedData))
            {
                _profileData = loadedData;
                _profileData?.EnsureDataIntegrity();
                if (_profileData.AddDefaultAvatarIds() || _profileData.AddDefaultBackgroundIds())
                {
                    SaveProfile();
                }
                NotifyProfileChanged();
                return true;
            }

            return false;
        }

        public void CreateNewProfile(string playerName)
        {
            _profileData = new ProfileData(playerName);
            _profileData.EnsureDataIntegrity();
            SaveProfile();
            NotifyProfileChanged();
        }

        public void SetPlayerName(string newName)
        {
            if (_profileData == null) return;
            if (string.IsNullOrEmpty(newName.Trim())) return;

            _profileData.SetPlayerName(newName);
            SaveProfile();
            NotifyProfileChanged();
        }

        public void AddCoins(int amount)
        {
            if (_profileData == null) return;

            _profileData.AddCoins(amount);
            SaveProfile();
            NotifyProfileChanged();
        }

        public bool AddExperience(int amount)
        {
            if (_profileData == null) return false;
            bool leveledUp = _profileData.AddExperience(amount);
            SaveProfile();
            NotifyProfileChanged();
            return leveledUp;
        }

        public void SetAvatar(string avatarId)
        {
            if (_profileData == null) return;
            _profileData.SetAvatar(avatarId);
            SaveProfile();
            NotifyProfileChanged();
        }

        public void SetBackground(string backgroundId)
        {
            if (_profileData == null) return;
            _profileData.SetBackground(backgroundId);
            SaveProfile();
            NotifyProfileChanged();
        }

        public void SaveProfile()
        {
            if (_profileData == null) return;
            _repository.Save(_profileData);
        }

        public bool TrySpendCoins(int amount)
        {
            if (_profileData == null) return false;

            bool spent = _profileData.TrySpendCoins(amount);
            if (!spent) return false;

            SaveProfile();
            NotifyProfileChanged();
            return true;
        }
        
        public bool HasAvatar(string avatarId)
        {
            if (_profileData == null) return false;
            return _profileData.HasAvatar(avatarId);
        }

        public bool HasBackground(string backgroundId)
        {
            if (_profileData == null) return false;
            return _profileData.HasBackground(backgroundId);
        }
        
        public bool TryAddAvatar(string avatarId)
        {
            if (_profileData == null) return false;
            
            bool added = _profileData.TryAddAvatar(avatarId);
            if (!added) return false;
            
            SaveProfile();
            NotifyProfileChanged();
            return true;
        }

        public bool TryAddBackground(string backgroundId)
        {
            if (_profileData == null) return false;
            
            bool added = _profileData.TryAddBackground(backgroundId);
            if (!added) return false;
            
            SaveProfile();
            NotifyProfileChanged();
            return true;
        }

        public bool TryAddBadgeItem(string itemId)
        {
            if (_profileData == null) return false;

            bool added = _profileData.TryAddBadgeItem(itemId);
            if (!added) return false;

            SaveProfile();
            NotifyProfileChanged();
            return true;
        }
        
        public bool TryAddBoosterItem(string itemId)
        {
            if (_profileData == null) return false;

            bool added = _profileData.TryAddBoosterItem(itemId);
            if (!added) return false;

            SaveProfile();
            NotifyProfileChanged();
            return true;
        }

        public void RegisterDailyLogin()
        {
            if (_profileData == null) return;

            bool updated = _profileData.RegisterDailyLogin(DateTime.Now);
            if (!updated) return;

            SaveProfile();
            NotifyProfileChanged();
        }

        private void NotifyProfileChanged()
        {
            ProfileChanged?.Invoke(_profileData);
        }

        public void RegisterCompletedLesson()
        {
            if (_profileData == null) return;

            _profileData.RegisterCompletedLesson();
            SaveProfile();
            NotifyProfileChanged();
        }

        public void RegisterIncompleteLesson()
        {
            if (_profileData == null) return;

            _profileData.RegisterIncompleteLesson();
            SaveProfile();
            NotifyProfileChanged();
        }

        public void RegisterShopPurchase(int spentCoins)
        {
            if (_profileData == null) return;

            _profileData.RegisterShopPurchase(spentCoins);
            SaveProfile();
            NotifyProfileChanged();
        }
    }
}
