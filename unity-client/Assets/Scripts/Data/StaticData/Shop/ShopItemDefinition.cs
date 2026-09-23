using System;
using Enums;
using UnityEngine;

namespace Data.StaticData.Shop
{
    [Serializable]
    public class ShopItemDefinition
    {
        public string Id;
        public Sprite Icon;
        public string Title;
        public string Description;
        public int Price;
        public ShopItemCategory Category;
    }
}