using System.Collections.Generic;
using Data.StaticData;
using Enums;

namespace Services
{
    public interface IProfileItemsService
    {
        List<ItemDefinition> GetTopItems(ProfileData profileData, ProfileItemCateogory profileItemCateogory,
            int count = 4);
    }
}