using System.Collections.Generic;
using Data.StaticData.Item;
using Enums;

namespace Services.Interfaces
{
    public interface IProfileItemsService
    {
        IReadOnlyList<ProfileItemDefinition> GetTopItems(ProfileItemCategory profileItemCategory,
            int count = 4);

        IReadOnlyList<ProfileItemDefinition> GetAllItems(ProfileItemCategory profileItemCategory);
    }
}
