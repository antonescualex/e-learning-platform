using System;
using System.Collections.Generic;

namespace Auth
{
    [Serializable]
    public sealed class ProfileDto
    {
        public string UserId;
        public string Username;
        public string PlayerName;
        public int LevelNumber;
        public int CurrentExperience;
        public int ExperienceNeededForNextLevel;
        public int Coins;
        public string SelectedAvatarId;
        public string SelectedBackgroundId;
        public string CreatedAt;
        public string LastLoginDate;
        public int CurrentLoginStreak;
        public int CompletedLessonsCount;
        public int IncompleteLessonsCount;
        public int TotalShopPurchases;
        public int TotalCoinsSpentInShop;
        public string DoubleCoinsExpiresAt;
        public string DoubleXpExpiresAt;
        public List<string> OwnedBadgeIds;
        public List<string> OwnedAvatarIds;
        public List<string> OwnedBackgroundIds;
        public List<BoosterDto> Boosters;
    }

    [Serializable]
    public sealed class BoosterDto
    {
        public string BoosterItemId;
        public int Quantity;
    }
}