using System;
using Enums;
using UnityEngine;

namespace Data.StaticData.Item
{
    [Serializable]
    public abstract class ProfileItemDefinition
    {
        public string Id;
        public string DisplayName;
        public Sprite Icon;

        public abstract ProfileItemCategory Category { get; }
    }
}
