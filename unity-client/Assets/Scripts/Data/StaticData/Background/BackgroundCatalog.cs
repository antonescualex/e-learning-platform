using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data.StaticData.Background
{
    [CreateAssetMenu(menuName = "Background Catalog", fileName = "BackgroundCatalog")]
    public class BackgroundCatalog : ScriptableObject
    {
        [SerializeField] private List<BackgroundDefinition> backgrounds = new List<BackgroundDefinition>();

        public BackgroundDefinition GetBackgroundDefinition(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            foreach (var background in backgrounds)
            {
                if (background.id == id) return background;
            }

            return null;
        }

        public bool ContainsId(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;

            foreach (var background in backgrounds)
            {
                if (background.id == id) return true;
            }

            return false;
        }
    }
}