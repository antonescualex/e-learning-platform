using System;
using System.Collections.Generic;
using Data.StaticData.Item;
using Enums;

namespace Services.Interfaces
{
    public interface IBoosterService
    {
        IReadOnlyList<ProfileItemDefinition> GetBoosters();
        bool TryActivateBooster(string boosterId);
        int GetRewardMultiplier(BoosterType boosterType);
        TimeSpan GetRemainingTime(BoosterType boosterType);
        bool IsBoosterActive(BoosterType boosterType);
        bool IsBoosterActive();
    }
}