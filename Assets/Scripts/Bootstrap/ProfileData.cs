using System;
using UnityEngine;

[Serializable]
public class ProfileData
{
    [SerializeField]private string _id;
    [SerializeField]private string _playerName;
    [SerializeField]private float _level;
    [SerializeField]private int _coins;

    public string Id => _id;
    public string PlayerName => _playerName;
    public float Level => _level;
    public int Coins => _coins;

    public ProfileData(string playerName)
    {
        _id = Guid.NewGuid().ToString();
        _playerName = playerName;
        _level = 1F;
        _coins = 0;
    }
    
    public void AddCoins(int amountToAdd)
    {
        _coins += amountToAdd;
    }

    public void IncreaseLevel(float amountToAdd)
    {
        _level += amountToAdd;
    }
    
}
