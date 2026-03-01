using System;
using Repositories;
using Services;

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
            _profileData.ShowDemoItems();
            SaveProfile();
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
        
        _profileData.SetPlayerName(newName);
        SaveProfile();
        ProfileChanged?.Invoke(_profileData);
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

    public void SaveProfile()
    {
        if (_profileData == null) return;
        _repository.Save(_profileData);
    }

    private void NotifyProfileChanged()
    {
        ProfileChanged?.Invoke(_profileData);
    }
}
