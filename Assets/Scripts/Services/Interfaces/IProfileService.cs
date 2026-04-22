using System;
using Data;

namespace Services.Interfaces
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
        void SetAvatar(string avatarId);
        void SetBackground(string backgroundId);

        bool TrySpendCoins(int amount);
        bool HasAvatar(string avatarId);
        bool HasBackground(string backgroundId);
        bool TryAddAvatar(string avatarId);
        bool TryAddBackground(string backgroundId);
        bool TryAddBadgeItem(string itemId);
        bool TryAddBoosterItem(string itemId);

        void RegisterDailyLogin();
        void RegisterCompletedLesson();
        void RegisterIncompleteLesson();
        void RegisterShopPurchase(int spentCoins);

        void SaveProfile();
    }
}
