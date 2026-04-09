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

        bool TrySpendCoins(int amount);
        bool HasAccessory(string accessoryId);
        bool TryAddAccessory(string accessoryId);
        bool TryAddBadgeItem(string itemId);

        void RegisterDailyLogin();
        void RegisterCompletedLesson(bool receivedSpecialItem);
        void RegisterIncompleteLesson();
        void RegisterShopPurchase(int spentCoins);

        bool TryAddBoosterItem(string itemId);
        bool TryAddRewardItem(string itemId);

        void SaveProfile();
    }
}
