using System.Collections.Generic;
using Data.StaticData;
using Data.StaticData.Item;

namespace Services.Interfaces
{
    public interface IInventoryService
    {
        IReadOnlyList<InventoryItem> GetItems();
    }
}