using System;
using System.Collections.Generic;
using System.Globalization;
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

        private const string LoginDateFormat = "yyyy-MM-dd";

        public string Id => _id;
        public string PlayerName => _playerName;
        public int Level => _level;
        public int CurrentExperience => _currentExperience;
        public int Coins => _coins;
        public string AvatarId => _avatarId;
        public string BackgroundId => _backgroundId;
        public string CreatedAt => _createdAt;
        public int CurrentLoginStreak => _currentLoginStreak;
        public int TotalShopPurchases => _totalShopPurchases;
        public int TotalCoinsSpentInShop => _totalCoinsSpentInShop;

        public IReadOnlyList<string> BadgeItemIds => _badgeItemIds;
        public IReadOnlyList<string> BoosterItemIds => _boosterItemIds;
        public IReadOnlyList<string> AvatarItemIds => _avatarItemIds;
        public IReadOnlyList<string> BackgroundItemIds => _backgroundItemIds;

        public int ExperienceToNextLevel => CalculateExperienceNeededForNextLevel(_level);
        public int CompletedLessonsCount => _completedLessonsCount;
        public int IncompleteLessonsCount => _incompleteLessonsCount;

        public ProfileData(string playerName)
        {
            _id = Guid.NewGuid().ToString();
            _playerName = playerName;
            _level = 1;
            _avatarId = "boy_3";
            _backgroundId = "default_background";
            _currentExperience = 0;
            _coins = 0;
            _createdAt = DateTime.Now.ToString("d");
            RegisterDailyLogin(DateTime.Now);
            AddDefaultAvatarIds();
            AddDefaultBackgroundIds();
        }

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
        
        public IReadOnlyList<string> GetAvatarIds()
        {
            return AvatarItemIds;
        }
        
        public IReadOnlyList<string> GetBackgroundIds()
        {
            return BackgroundItemIds;
        }

        public void SetPlayerName(string newName)
        {
            if (string.IsNullOrEmpty(newName)) return;

            _playerName = newName.Trim();
        }

        public void AddCoins(int amount)
        {
            if (amount <= 0) return;
            _coins += amount;
        }

        public bool AddExperience(int amount)
        {
            if (amount <= 0) return false;

            _currentExperience += amount;
            bool leveledUp = false;

            while (_currentExperience >= ExperienceToNextLevel)
            {
                _currentExperience -= ExperienceToNextLevel;
                _level++;
                leveledUp = true;
            }

            return leveledUp;
        }

        public void SetAvatar(string avatarId)
        {
            if (string.IsNullOrEmpty(avatarId)) return;
            _avatarId = avatarId;
        }
        
        public void SetBackground(string backgroundId)
        {
            if (string.IsNullOrEmpty(backgroundId)) return;
            _backgroundId = backgroundId;
        }

        public bool TrySpendCoins(int amount)
        {
            if (amount <= 0) return false;
            if (_coins < amount) return false;

            _coins -= amount;
            return true;
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
        
        public bool TryAddAvatar(string avatarId)
        {
            if (string.IsNullOrEmpty(avatarId)) return false;
            if (_avatarItemIds.Contains(avatarId)) return false;
            
            _avatarItemIds.Add(avatarId);
            return true;
        }
        
        public bool TryAddBackground(string backgroundId)
        {
            if (string.IsNullOrEmpty(backgroundId)) return false;
            if (_backgroundItemIds.Contains(backgroundId)) return false;
            
            _backgroundItemIds.Add(backgroundId);
            return true;
        }

        public bool AddDefaultAvatarIds()
        {
            bool changed = false;
            List<string> defaultAvatarIds = new List<string> { "boy_3", "girl_1", "boy_1", "girl_3", "boy_4", "girl_2", "boy_2", "girl_4" };
            foreach (var avatarId in defaultAvatarIds)
            {
                if (TryAddAvatar(avatarId))
                {
                    changed = true;
                }
            }
            return changed;
        }

        public bool AddDefaultBackgroundIds()
        {
            return TryAddBackground("default_background");
        }

        private static int CalculateExperienceNeededForNextLevel(int currentLevel)
        {
            int baseExperience = 100;
            float growth = 1.5f;

            return Mathf.RoundToInt(baseExperience * Mathf.Pow(growth, currentLevel - 1));
        }

        public void RegisterCompletedLesson()
        {
            _completedLessonsCount++;
        }

        public void RegisterIncompleteLesson()
        {
            _incompleteLessonsCount++;
        }

        public bool RegisterDailyLogin(DateTime currentDate)
        {
            currentDate = currentDate.Date;

            if (!TryParseStoredDate(_lastLoginDate, out DateTime lastLoginDate))
            {
                _lastLoginDate = FormatStoredDate(currentDate);
                _currentLoginStreak = 1;
                return true;
            }

            if (currentDate <= lastLoginDate)
            {
                return false;
            }

            _currentLoginStreak = currentDate == lastLoginDate.AddDays(1)
                ? Mathf.Max(1, _currentLoginStreak + 1)
                : 1;

            _lastLoginDate = FormatStoredDate(currentDate);
            return true;
        }

        public void RegisterShopPurchase(int spentCoins)
        {
            if (spentCoins <= 0) return;

            _totalShopPurchases++;
            _totalCoinsSpentInShop += spentCoins;
        }

        public bool TryAddBadgeItem(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return false;
            if (_badgeItemIds.Contains(itemId)) return false;

            _badgeItemIds.Add(itemId);
            return true;
        }

        public bool TryAddBoosterItem(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return false;
            _boosterItemIds.Add(itemId);
            return true;
        }
        
        public bool TryRemoveBoosterItem(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return false;

            return _boosterItemIds.Remove(itemId);
        }
        
        public void ActivateBooster(BoosterType boosterType, int durationSeconds)
        {
            DateTime now = DateTime.Now;
            DateTime currentExpiration = GetBoosterExpiration(boosterType);

            DateTime startTime = currentExpiration > now ? currentExpiration : now;
            DateTime newExpiration = startTime.AddSeconds(durationSeconds);

            SetBoosterExpiration(boosterType, newExpiration);
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

        private void SetBoosterExpiration(BoosterType boosterType, DateTime expiration)
        {
            string value = expiration.ToString();

            if (boosterType == BoosterType.DoubleCoins)
            {
                _doubleCoinsBoosterExirialDate = value;
            }
            else
            {
                _doubleXpBoosterExirialDate = value;
            }
        }

        private static bool TryParseStoredDate(string value, out DateTime result)
        {
            return DateTime.TryParseExact(
                value,
                LoginDateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out result);
        }

        private static string FormatStoredDate(DateTime date)
        {
            return date.ToString(LoginDateFormat, CultureInfo.InvariantCulture);
        }
    }
}
