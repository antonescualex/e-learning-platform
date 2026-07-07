using System.Collections.Generic;
using Data.StaticData.Abstractions;
using UnityEngine;

namespace Data.StaticData.Booster
{
    [CreateAssetMenu(menuName = "Catalogs/Profile Items/Boosters Catalog")]
    public class BoosterCatalog : ProfileItemCatalogBase
    {
        [SerializeField] private List<BoosterDefinition> items = new List<BoosterDefinition>();

        public IReadOnlyList<BoosterDefinition> BoosterDefinitions => items;
        public override IReadOnlyList<ProfileItemDefinition> Items => items;

        public BoosterDefinition GetBoosterById(string id)
        {
            return GetById(id) as BoosterDefinition;
        }
    }
}
