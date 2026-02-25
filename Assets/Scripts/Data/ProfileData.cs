using System;
using UnityEngine;

[Serializable]
public class ProfileData
{
    [SerializeField] private string _id;
    [SerializeField] private string _playerName;
    [SerializeField] private int _level;
    [SerializeField] private int _currentExperience;
    [SerializeField] private int _coins;

    public string Id => _id;
    public string PlayerName => _playerName;
    public int Level => _level;
    public int CurrentExperience => _currentExperience;
    public int Coins => _coins;

    public int ExperienceToNextLevel => CalculateExperienceNeededForNextLevel(_level);

    public ProfileData(string playerName)
    {
        _id = Guid.NewGuid().ToString();
        _playerName = playerName;
        _level = 1;
        _currentExperience = 0;
        _coins = 0;
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

    private static int CalculateExperienceNeededForNextLevel(int currentLevel)
    {
        int baseExperience = 100;
        float growth = 1.5f;

        return Mathf.RoundToInt(baseExperience * Mathf.Pow(growth, currentLevel - 1));
    }
    
}
