using System.Collections.Generic;
using Data.StaticData;

namespace Services
{
    public interface IInventoryService
    {
        IReadOnlyList<InventoryItem> GetItems();
    }
}