using System;
using Enums;
using UnityEngine;

namespace Data.StaticData.Abstractions
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
