using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

[Serializable]
public class ProfileData
{
    [SerializeField] private string _id;
    [SerializeField] private string _playerName;
    [SerializeField] private int _level;
    [SerializeField] private int _currentExperience;
    [SerializeField] private int _coins;
    [SerializeField] private string _avatarId;
    [SerializeField] private string _createdAt;
    [SerializeField] private int _completedLessonsCount;
    [SerializeField] private int _incompleteLessonsCount;
    [SerializeField] private int _lessonsSinceLastSpecialItemDrop;

    [SerializeField] private List<string> _badgeItemIds = new List<string>();
    [SerializeField] private List<string> _boosterItemIds = new List<string>();
    [SerializeField] private List<string> _rewardItemIds = new List<string>();
    [SerializeField] private List<string> _accessoryItemIds = new List<string>();

    public string Id => _id;
    public string PlayerName => _playerName;
    public int Level => _level;
    public int CurrentExperience => _currentExperience;
    public int Coins => _coins;
    public string AvatarId => _avatarId;
    public string CreatedAt => _createdAt;

    public IReadOnlyList<string> BadgeItemIds => _badgeItemIds;
    public IReadOnlyList<string> BoosterItemIds => _boosterItemIds;
    public IReadOnlyList<string> RewardItemIds => _rewardItemIds;
    public IReadOnlyList<string> AccessoryItemIds => _accessoryItemIds;

    public int ExperienceToNextLevel => CalculateExperienceNeededForNextLevel(_level);
    public int CompletedLessonsCount => _completedLessonsCount;
    public int IncompleteLessonsCount => _incompleteLessonsCount;
    public int LessonsSinceLastSpecialItemDrop => _lessonsSinceLastSpecialItemDrop;

    public ProfileData(string playerName)
    {
        _id = Guid.NewGuid().ToString();
        _playerName = playerName;
        _level = 1;
        _currentExperience = 0;
        _coins = 0;
        _createdAt = DateTime.Now.ToString("d");
    }

    public IReadOnlyList<string> GetItemIds(ProfileItemCateogory profileItemCateogory)
    {
        switch (profileItemCateogory)
        {
            case ProfileItemCateogory.Badges:
                return _badgeItemIds;
            case ProfileItemCateogory.Boosters:
                return _boosterItemIds;
            case ProfileItemCateogory.Rewards:
                return _rewardItemIds;
            default:
                return _badgeItemIds;
        }
    }

    public IReadOnlyList<string> GetAccessoryIds()
    {
        return AccessoryItemIds;
    }

    public void ShowDemoItems()
    {
        if (_badgeItemIds.Count == 0)
        {
            _badgeItemIds.AddRange(new[] { "badge_lightning", "badge_rocket", "badge_time", "badge_moves" });
        }
        if (_boosterItemIds.Count == 0)
        {
            _boosterItemIds.AddRange(new[] { "booster_double_xp_10m", "booster_double_xp_1h", "booster_double_coins_10m", "booster_double_coins_1h" });
        }
        if (_rewardItemIds.Count == 0)
        {
            _rewardItemIds.AddRange(new[] { "reward_common", "reward_rare", "reward_epic", "reward_legendary" });
        }
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

    public bool TrySpendCoins(int amount)
    {
        if (amount <= 0) return false;
        if (_coins < amount) return false;

        _coins -= amount;
        return true;
    }

    public bool HasAccessory(string accessoryId)
    {
        if (string.IsNullOrEmpty(accessoryId)) return false;
        return _accessoryItemIds.Contains(accessoryId);
    }

    public bool TryAddAccessory(string accessoryId)
    {
        if (string.IsNullOrEmpty(accessoryId)) return false;
        if (_accessoryItemIds.Contains(accessoryId)) return false;

        _accessoryItemIds.Add(accessoryId);
        return true;
    }

    private static int CalculateExperienceNeededForNextLevel(int currentLevel)
    {
        int baseExperience = 100;
        float growth = 1.5f;

        return Mathf.RoundToInt(baseExperience * Mathf.Pow(growth, currentLevel - 1));
    }

    public void RegisterCompletedLesson(bool receivedSpecialItem)
    {
        _completedLessonsCount++;
        _lessonsSinceLastSpecialItemDrop = receivedSpecialItem ? 0 : _lessonsSinceLastSpecialItemDrop + 1;
    }

    public void RegisterIncompleteLesson()
    {
        _incompleteLessonsCount++;
    }

    public bool TryAddBoosterItem(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return false;
        _boosterItemIds.Add(itemId);
        return true;
    }

    public bool TryAddRewardItem(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return false;
        _rewardItemIds.Add(itemId);
        return true;
    }


}
