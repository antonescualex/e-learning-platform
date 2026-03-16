using System.Collections.Generic;
using UnityEngine;

namespace Data.StaticData.Avatar
{
    [CreateAssetMenu(menuName = "Avatar Catalog", fileName = "AvatarCatalog")]
    public class AvatarCatalog : ScriptableObject
    {
        [SerializeField] private List<AvatarDefinition> avatars = new List<AvatarDefinition>();

        public Sprite GetSprite(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            foreach (var avatar in avatars)
            {
                if (avatar.id == id) return avatar.sprite;
            }

            return null;
        }

        public bool ContainsId(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;

            foreach (var avatar in avatars)
            {
                if (avatar.id == id) return true;
            }

            return false;
        }
    }
}