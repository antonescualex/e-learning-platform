using System.Collections.Generic;
using UnityEngine;

namespace Data.StaticData.Item
{
    [CreateAssetMenu(menuName = "ItemCatalog")]
    public class ItemCatalog : ScriptableObject
    {
        [SerializeField] public List<ItemDefinition> items = new List<ItemDefinition>();

        public IReadOnlyList<ItemDefinition> Items => items;

        private Dictionary<string, ItemDefinition> _dictionary;

        public ItemDefinition GetById(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            if (_dictionary == null)
            {
                _dictionary = BuildDictionary();
            }

            _dictionary.TryGetValue(id, out var itemDefinition);
            return itemDefinition;
        }

        private Dictionary<string, ItemDefinition> BuildDictionary()
        {
            Dictionary<string, ItemDefinition> dictionary = new Dictionary<string, ItemDefinition>();
            foreach (var item in Items)
            {
                if (item == null || string.IsNullOrEmpty(item.Id)) continue;
                if (!dictionary.ContainsKey(item.Id))
                {
                    dictionary.Add(item.Id, item);
                }
            }

            return dictionary;
        }
    }
}