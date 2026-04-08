using System;
using Enums;
using UnityEngine;
using UnityEngine.Serialization;

namespace Data.StaticData.Item
{
    [Serializable]
    public class ItemDefinition : ProfileItemDefinition
    {
        [FormerlySerializedAs("Cateogory")] public ProfileItemCategory category;

        [Header("Booster")]
        public BoosterType BoosterType;
        public int DurationSeconds;

        [Header("Rewards")] public RewardType RewardType;

        public override ProfileItemCategory Category => category;
    }
}
