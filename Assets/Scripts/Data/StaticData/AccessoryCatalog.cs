using System.Collections.Generic;
using UnityEngine;

namespace Data.StaticData
{
    [CreateAssetMenu(menuName = "AccessoryCatalog")]
    public class AccessoryCatalog : ScriptableObject
    {
        [SerializeField] private List<AccessoryDefinition> accessoryDefinitions = new();

        private Dictionary<string, AccessoryDefinition> _dictionary;

        public IReadOnlyList<AccessoryDefinition> AccessoryDefinitions => accessoryDefinitions;

        public AccessoryDefinition GetById(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            if (_dictionary == null)
            {
                _dictionary = BuildDictionary();
            }

            _dictionary.TryGetValue(id, out var accessoryDefinition);
            return accessoryDefinition;
        }
        
        private Dictionary<string, AccessoryDefinition> BuildDictionary()
        {
            Dictionary<string, AccessoryDefinition> dictionary = new();

            foreach (var accessoryDefinition in AccessoryDefinitions)
            {
                if(accessoryDefinition == null || string.IsNullOrEmpty(accessoryDefinition.Id)) continue;
                if (!dictionary.ContainsKey(accessoryDefinition.Id))
                {
                    dictionary.Add(accessoryDefinition.Id, accessoryDefinition);
                }
            }

            return dictionary;
        }
    }
}