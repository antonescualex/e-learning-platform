using System;
using Enums;
using UnityEngine;

namespace Data.StaticData
{
    [Serializable]
    public class ItemDefinition
    {
        public string Id;
        public ProfileItemCateogory Cateogory;

        public string DisplayName;
        public Sprite Icon;

        [Header("Booster")]
        public BoosterType BoosterType;
        public int DurationSeconds;

        [Header("Rewards")] public RewardType RewardType;
    }
}