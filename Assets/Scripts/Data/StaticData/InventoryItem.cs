using UnityEngine;

namespace Data.StaticData
{
    public class InventoryItem
    {
        public string Id { get; }
        public string Title { get; }
        public Sprite Icon { get; }

        public InventoryItem(string id, string title, Sprite icon)
        {
            Id = id;
            Title = title;
            Icon = icon;
        }
    }
}