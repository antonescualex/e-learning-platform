using System;
using Enums;

namespace Data.StaticData.Item
{
    [Serializable]
    public sealed class BadgeDefinition : ProfileItemDefinition
    {
        public string Description;

        public override ProfileItemCategory Category => ProfileItemCategory.Badges;
    }
}
