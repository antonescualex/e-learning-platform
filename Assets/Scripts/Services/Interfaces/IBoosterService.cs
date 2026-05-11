using System;
using System.Collections;
using System.Collections.Generic;
using Data.StaticData.Item;
using Enums;

namespace Services.Interfaces
{
    public interface IBoosterService
    {
        IReadOnlyList<ProfileItemDefinition> GetBoosters();
        IEnumerator ActivateBooster(
            string boosterItemId,
            Action onSuccess,
            Action<string> onError);
        int GetRewardMultiplier(BoosterType boosterType);
        TimeSpan GetRemainingTime(BoosterType boosterType);
        bool IsBoosterActive(BoosterType boosterType);
        bool IsBoosterActive();
    }
}