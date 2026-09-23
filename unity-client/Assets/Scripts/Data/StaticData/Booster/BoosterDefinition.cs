using System;
using Data.StaticData.Abstractions;
using Enums;

namespace Data.StaticData.Booster
{
    [Serializable]
    public sealed class BoosterDefinition : ProfileItemDefinition
    {
        public BoosterType BoosterType;
        public int DurationSeconds;

        public override ProfileItemCategory Category => ProfileItemCategory.Boosters;
    }
}
