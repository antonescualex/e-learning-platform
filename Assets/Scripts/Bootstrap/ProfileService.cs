using System.IO;
using UnityEngine;

public class ProfileService
{
    private const string ProfileFileName = "profile.json";
    private static readonly string defaultPlayerName = "MyPlayer";
    
    private ProfileData _profileData;
    public ProfileData Data => _profileData;

    public void InitializeProfile()
    {
        _profileData = DataService.Load<ProfileData>(DataService.ProfilesFolder, ProfileFileName);

        if (_profileData == null)
        {
            _profileData = new ProfileData(defaultPlayerName);
            SaveProfile();
            Debug.Log($"Created new profile");
        }
        else
        {
            Debug.Log($"Loaded existing profile: {_profileData.PlayerName}, Level: {_profileData.Level}, Coins: {_profileData.Coins}");
        }
    }

    private void AddCoins(int amount)
    {
        if (amount > 0)
        {
            _profileData.AddCoins(amount);
            SaveProfile();
        }
    }

    private void IncreaseLevel(float amount)
    {
        if (amount > 0F)
        {
            _profileData.IncreaseLevel(amount);
            SaveProfile();
        }
    }

    public void SaveProfile()
    {
        DataService.Save(DataService.ProfilesFolder, ProfileFileName, _profileData);
    }
}
