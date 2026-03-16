using System.Collections.Generic;
using Data.StaticData;
using Data.StaticData.Item;
using Enums;

namespace Services
{
    public interface IProfileItemsService
    {
        IReadOnlyList<ItemDefinition> GetTopItems(ProfileItemCateogory profileItemCateogory,
            int count = 4);

        IReadOnlyList<ItemDefinition> GetAllItems(ProfileItemCateogory profileItemCateogory);
    }
}