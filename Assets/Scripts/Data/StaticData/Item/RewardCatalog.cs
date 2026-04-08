using System.Collections.Generic;
using UnityEngine;

namespace Data.StaticData.Item
{
    [CreateAssetMenu(menuName = "Catalogs/Profile Items/Rewards Catalog")]
    public class RewardCatalog : ProfileItemCatalogBase
    {
        [SerializeField] private List<RewardDefinition> items = new List<RewardDefinition>();

        public IReadOnlyList<RewardDefinition> RewardDefinitions => items;
        public override IReadOnlyList<ProfileItemDefinition> Items => items;

        public RewardDefinition GetRewardById(string id)
        {
            return GetById(id) as RewardDefinition;
        }
    }
}
