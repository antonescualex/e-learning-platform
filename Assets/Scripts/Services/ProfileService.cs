using Repositories;

public class ProfileService
{
    private readonly IRepository<ProfileData> _repository;
    private ProfileData _profileData;

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
            return true;
        }

        return false;

        // _profileData = DataService.Load<ProfileData>(DataService.ProfilesFolder, ProfileFileName);
        // if (_profileData != null)
        // {
        //     Debug.Log($"Loaded existing profile: {_profileData.PlayerName}, Level: {_profileData.Level}, Coins: {_profileData.Coins}");
        //     return true;
        // }
        //
        // return false;
    }

    public void CreateNewProfile(string playerName)
    {
        _profileData = new ProfileData(playerName);
        SaveProfile();
    }

    public void AddCoins(int amount)
    {
        if (_profileData == null) return;
        
        _profileData.AddCoins(amount);
        SaveProfile();
    }

    public bool AddExperience(int amount)
    {
        if (_profileData == null) return false;
        bool leveledUp = _profileData.AddExperience(amount);
        SaveProfile();
        return leveledUp;
    }

    public void SaveProfile()
    {
        if (_profileData == null) return;
        _repository.Save(_profileData);
    }
}
