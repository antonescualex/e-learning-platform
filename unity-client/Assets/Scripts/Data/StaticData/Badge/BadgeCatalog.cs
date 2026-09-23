using System.Collections.Generic;
using Data.StaticData.Abstractions;
using UnityEngine;

namespace Data.StaticData.Badge
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
