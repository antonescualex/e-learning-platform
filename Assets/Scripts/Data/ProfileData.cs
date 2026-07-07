using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

namespace Data
{
    [Serializable]
    public class ProfileData
    {
        [SerializeField] private string _id;
        [SerializeField] private string _playerName;
        [SerializeField] private int _level;
        [SerializeField] private int _currentExperience;
        [SerializeField] private int _experienceNeededForNextLevel;
        [SerializeField] private int _coins;
        [SerializeField] private string _avatarId;
        [SerializeField] private string _backgroundId;
        [SerializeField] private string _createdAt;
        [SerializeField] private string _lastLoginDate;
        [SerializeField] private int _currentLoginStreak;
        [SerializeField] private int _completedLessonsCount;
        [SerializeField] private int _incompleteLessonsCount;
        [SerializeField] private int _totalShopPurchases;
        [SerializeField] private int _totalCoinsSpentInShop;
        [SerializeField] private string _doubleCoinsBoosterExirialDate;
        [SerializeField] private string _doubleXpBoosterExirialDate;

        [SerializeField] private List<string> _badgeItemIds = new List<string>();
        [SerializeField] private List<string> _boosterItemIds = new List<string>();
        [SerializeField] private List<string> _avatarItemIds = new List<string>();
        [SerializeField] private List<string> _backgroundItemIds = new List<string>();

        public string Id => _id;
        public string PlayerName => _playerName;
        public int Level => _level;
        public int CurrentExperience => _currentExperience;
        public int Coins => _coins;
        public string AvatarId => _avatarId;
        public string BackgroundId => _backgroundId;

        public string CreatedAt
        {
            get
            {
                if(string.IsNullOrEmpty(_createdAt)) return string.Empty;
                if(DateTimeOffset.TryParse(_createdAt, out DateTimeOffset date)) return date.ToString("dd.MM.yyyy");
                return _createdAt;
            }
        }

        public int CurrentLoginStreak => _currentLoginStreak;
        public int TotalShopPurchases => _totalShopPurchases;
        public int TotalCoinsSpentInShop => _totalCoinsSpentInShop;

        public IReadOnlyList<string> BadgeItemIds => _badgeItemIds;
        public IReadOnlyList<string> BoosterItemIds => _boosterItemIds;
        public IReadOnlyList<string> AvatarItemIds => _avatarItemIds;
        public IReadOnlyList<string> BackgroundItemIds => _backgroundItemIds;

        public int ExperienceToNextLevel => _experienceNeededForNextLevel;
        public int CompletedLessonsCount => _completedLessonsCount;
        public int IncompleteLessonsCount => _incompleteLessonsCount;

        public void EnsureDataIntegrity()
        {
            _badgeItemIds ??= new List<string>();
            _boosterItemIds ??= new List<string>();
            _avatarItemIds ??= new List<string>();
            _backgroundItemIds ??= new List<string>();
            _currentLoginStreak = Mathf.Max(0, _currentLoginStreak);
            _completedLessonsCount = Mathf.Max(0, _completedLessonsCount);
            _incompleteLessonsCount = Mathf.Max(0, _incompleteLessonsCount);
            _totalShopPurchases = Mathf.Max(0, _totalShopPurchases);
            _totalCoinsSpentInShop = Mathf.Max(0, _totalCoinsSpentInShop);
        }

        public IReadOnlyList<string> GetItemIds(ProfileItemCategory profileItemCategory)
        {
            switch (profileItemCategory)
            {
                case ProfileItemCategory.Badges:
                    return _badgeItemIds;
                case ProfileItemCategory.Boosters:
                    return _boosterItemIds;
                default:
                    return _badgeItemIds;
            }
        }

        public bool HasAvatar(string avatarId)
        {
            if (string.IsNullOrEmpty(avatarId)) return false;
            return _avatarItemIds.Contains(avatarId);
        }

        public bool HasBackground(string backgroundId)
        {
            if (string.IsNullOrEmpty(backgroundId)) return false;
            return _backgroundItemIds.Contains(backgroundId);
        }

        private static int CalculateExperienceNeededForNextLevel(int currentLevel)
        {
            int baseExperience = 100;
            float growth = 1.5f;

            return Mathf.RoundToInt(baseExperience * Mathf.Pow(growth, currentLevel - 1));
        }
        
        public bool IsBoosterActive(BoosterType boosterType)
        {
            return GetBoosterExpiration(boosterType) > DateTime.Now;
        }
        
        public TimeSpan GetBoosterRemainingTime(BoosterType boosterType)
        {
            DateTime expiration = GetBoosterExpiration(boosterType);

            if (expiration <= DateTime.Now)
            {
                return TimeSpan.Zero;
            }

            return expiration - DateTime.Now;
        }
        
        private DateTime GetBoosterExpiration(BoosterType boosterType)
        {
            string value = boosterType == BoosterType.DoubleCoins
                ? _doubleCoinsBoosterExirialDate
                : _doubleXpBoosterExirialDate;

            if (DateTime.TryParse(value, out DateTime expiration))
            {
                return expiration;
            }

            return DateTime.MinValue;
        }
    }
}
