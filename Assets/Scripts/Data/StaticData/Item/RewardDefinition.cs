using System;
using Enums;

namespace Data.StaticData.Item
{
    [Serializable]
    public sealed class RewardDefinition : ProfileItemDefinition
    {
        public RewardType RewardType;

        public override ProfileItemCategory Category => ProfileItemCategory.Rewards;
    }
}
