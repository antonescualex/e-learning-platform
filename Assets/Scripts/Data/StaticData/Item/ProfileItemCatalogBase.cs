using System.Collections.Generic;
using UnityEngine;

namespace Data.StaticData.Item
{
    public abstract class ProfileItemCatalogBase : ScriptableObject
    {
        private Dictionary<string, ProfileItemDefinition> _dictionary;

        public abstract IReadOnlyList<ProfileItemDefinition> Items { get; }

        public ProfileItemDefinition GetById(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            if (_dictionary == null)
            {
                _dictionary = BuildDictionary();
            }

            _dictionary.TryGetValue(id, out var itemDefinition);
            return itemDefinition;
        }

        protected virtual void OnEnable()
        {
            _dictionary = null;
        }

        protected virtual void OnValidate()
        {
            _dictionary = null;
        }

        private Dictionary<string, ProfileItemDefinition> BuildDictionary()
        {
            var dictionary = new Dictionary<string, ProfileItemDefinition>();
            var items = Items;
            if (items == null) return dictionary;

            foreach (var item in items)
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
