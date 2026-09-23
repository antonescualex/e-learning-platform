using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Data.StaticData.Shop
{
    [CreateAssetMenu(menuName = "Shop Catalog")]
    public class ShopCatalog : ScriptableObject
    {
        [FormerlySerializedAs("accessoryDefinitions")] [SerializeField] private List<ShopItemDefinition> shopItemDefinitions = new();

        private Dictionary<string, ShopItemDefinition> _dictionary;

        public IReadOnlyList<ShopItemDefinition> ShopItemDefinitions => shopItemDefinitions;

        public ShopItemDefinition GetById(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            if (_dictionary == null)
            {
                _dictionary = BuildDictionary();
            }

            _dictionary.TryGetValue(id, out var shopItemDefinition);
            return shopItemDefinition;
        }

        private Dictionary<string, ShopItemDefinition> BuildDictionary()
        {
            Dictionary<string, ShopItemDefinition> dictionary = new();

            foreach (var shopItemDefinition in ShopItemDefinitions)
            {
                if (shopItemDefinition == null || string.IsNullOrEmpty(shopItemDefinition.Id)) continue;
                if (!dictionary.ContainsKey(shopItemDefinition.Id))
                {
                    dictionary.Add(shopItemDefinition.Id, shopItemDefinition);
                }
            }

            return dictionary;
        }
    }
}