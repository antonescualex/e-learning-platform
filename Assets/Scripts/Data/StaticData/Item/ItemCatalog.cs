using System.Collections.Generic;
using UnityEngine;

namespace Data.StaticData.Item
{
    [CreateAssetMenu(menuName = "ItemCatalog")]
    public class ItemCatalog : ProfileItemCatalogBase
    {
        [SerializeField] public List<ItemDefinition> items = new List<ItemDefinition>();

        public IReadOnlyList<ItemDefinition> LegacyItems => items;
        public override IReadOnlyList<ProfileItemDefinition> Items => items;

        public ItemDefinition GetLegacyById(string id)
        {
            return GetById(id) as ItemDefinition;
        }
    }
}
