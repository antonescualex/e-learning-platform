using System;
using Data.StaticData.Abstractions;
using Enums;

namespace Data.StaticData.Badge
{
    [Serializable]
    public sealed class BadgeDefinition : ProfileItemDefinition
    {
        public string Description;

        public override ProfileItemCategory Category => ProfileItemCategory.Badges;
    }
}
