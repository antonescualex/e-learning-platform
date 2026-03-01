using System;

namespace Services
{
    public interface IProfileService
    {
        event Action<ProfileData> ProfileChanged;
        
        ProfileData ProfileData { get; }
        bool HasProfile { get; }

        bool TryLoadProfile();
        void CreateNewProfile(string playerName);

        void SetPlayerName(string newName);
        void AddCoins(int amount);
        bool AddExperience(int amount);

        void SaveProfile();
    }
}