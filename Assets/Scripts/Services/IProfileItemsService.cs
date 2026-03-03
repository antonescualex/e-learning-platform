using System.Collections.Generic;
using Data.StaticData;
using Enums;

namespace Services
{
    public interface IProfileItemsService
    {
        IReadOnlyList<ItemDefinition> GetTopItems(ProfileItemCateogory profileItemCateogory,
            int count = 4);
    }
}