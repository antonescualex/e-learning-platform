using System;
using UnityEngine;

namespace Data.StaticData.Accessory
{
    [Serializable]
    public class AccessoryDefinition
    {
        public string Id;
        public Sprite Icon;
        public string Title;
        public string Description;
        public int Price;
    }
}