using System;
using System.Collections.Generic;
using Data;
using UnityEngine;

namespace Auth
{
    public static class ProfileMapper
    {
        public static ProfileData ToProfileData(ProfileDto dto)
        {
            if (dto == null)
            {
                return null;
            }

            ProfileDataPayload payload = new ProfileDataPayload
            {
                _id = dto.UserId,
                _playerName = dto.PlayerName,
                _level = dto.LevelNumber,
                _currentExperience = dto.CurrentExperience,
                _experienceNeededForNextLevel = dto.ExperienceNeededForNextLevel,
                _coins = dto.Coins,
                _avatarId = dto.SelectedAvatarId,
                _backgroundId = dto.SelectedBackgroundId,
                _createdAt = dto.CreatedAt,
                _lastLoginDate = dto.LastLoginDate,
                _currentLoginStreak = dto.CurrentLoginStreak,
                _completedLessonsCount = dto.CompletedLessonsCount,
                _incompleteLessonsCount = dto.IncompleteLessonsCount,
                _totalShopPurchases = dto.TotalShopPurchases,
                _totalCoinsSpentInShop = dto.TotalCoinsSpentInShop,
                _doubleCoinsBoosterExirialDate = dto.DoubleCoinsExpiresAt,
                _doubleXpBoosterExirialDate = dto.DoubleXpExpiresAt,
                _badgeItemIds = dto.OwnedBadgeIds ?? new List<string>(),
                _boosterItemIds = ExpandBoosterIds(dto.Boosters),
                _avatarItemIds = dto.OwnedAvatarIds ?? new List<string>(),
                _backgroundItemIds = dto.OwnedBackgroundIds ?? new List<string>()
            };

            string json = JsonUtility.ToJson(payload);
            ProfileData profileData = JsonUtility.FromJson<ProfileData>(json);
            profileData?.EnsureDataIntegrity();
            return profileData;
        }

        private static List<string> ExpandBoosterIds(List<BoosterDto> boosters)
        {
            List<string> result = new List<string>();
            if (boosters == null)
            {
                return result;
            }

            foreach (BoosterDto booster in boosters)
            {
                if (booster == null || string.IsNullOrWhiteSpace(booster.BoosterItemId))
                {
                    continue;
                }

                int quantity = Math.Max(0, booster.Quantity);
                for (int i = 0; i < quantity; i++)
                {
                    result.Add(booster.BoosterItemId);
                }
            }

            return result;
        }

        [Serializable]
        private sealed class ProfileDataPayload
        {
            public string _id;
            public string _playerName;
            public int _level;
            public int _currentExperience;
            public int _experienceNeededForNextLevel;
            public int _coins;
            public string _avatarId;
            public string _backgroundId;
            public string _createdAt;
            public string _lastLoginDate;
            public int _currentLoginStreak;
            public int _completedLessonsCount;
            public int _incompleteLessonsCount;
            public int _totalShopPurchases;
            public int _totalCoinsSpentInShop;
            public string _doubleCoinsBoosterExirialDate;
            public string _doubleXpBoosterExirialDate;
            public List<string> _badgeItemIds;
            public List<string> _boosterItemIds;
            public List<string> _avatarItemIds;
            public List<string> _backgroundItemIds;
        }
    }
}