using System;
using Enums;

namespace Data.StaticData.Item
{
    [Serializable]
    public sealed class BoosterDefinition : ProfileItemDefinition
    {
        public BoosterType BoosterType;
        public int DurationSeconds;

        public override ProfileItemCategory Category => ProfileItemCategory.Boosters;
    }
}
