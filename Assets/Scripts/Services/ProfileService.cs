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
                NotifyProfileChanged();
                return true;
            }

            return false;
        }

        public void CreateNewProfile(string playerName)
        {
            _profileData = new ProfileData(playerName);
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

        public bool HasAccessory(string accessoryId)
        {
            if (_profileData == null) return false;
            return _profileData.HasAccessory(accessoryId);
        }

        public bool TryAddAccessory(string accessoryId)
        {
            if (_profileData == null) return false;

            bool added = _profileData.TryAddAccessory(accessoryId);
            if (!added) return false;

            SaveProfile();
            NotifyProfileChanged();
            return true;
        }

        private void NotifyProfileChanged()
        {
            ProfileChanged?.Invoke(_profileData);
        }

        public void RegisterCompletedLesson(bool receivedSpecialItem)
        {
            if (_profileData == null) return;

            _profileData.RegisterCompletedLesson(receivedSpecialItem);
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

        public bool TryAddBoosterItem(string itemId)
        {
            if (_profileData == null) return false;

            bool added = _profileData.TryAddBoosterItem(itemId);
            if (!added) return false;

            SaveProfile();
            NotifyProfileChanged();
            return true;
        }

        public bool TryAddRewardItem(string itemId)
        {
            if (_profileData == null) return false;

            bool added = _profileData.TryAddRewardItem(itemId);
            if (!added) return false;

            SaveProfile();
            NotifyProfileChanged();
            return true;
        }

    }
}
