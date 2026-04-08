using System.Collections.Generic;
using UnityEngine;

namespace Data.StaticData.Item
{
    [CreateAssetMenu(menuName = "Catalogs/Profile Items/Badges Catalog")]
    public class BadgeCatalog : ProfileItemCatalogBase
    {
        [SerializeField] private List<BadgeDefinition> items = new List<BadgeDefinition>();

        public IReadOnlyList<BadgeDefinition> BadgeDefinitions => items;
        public override IReadOnlyList<ProfileItemDefinition> Items => items;

        public BadgeDefinition GetBadgeById(string id)
        {
            return GetById(id) as BadgeDefinition;
        }
    }
}
