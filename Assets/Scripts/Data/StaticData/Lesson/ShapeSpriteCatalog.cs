using System.Collections.Generic;
using UnityEngine;

namespace Data.StaticData.Lesson
{
    [CreateAssetMenu(menuName = "Lessons/Shapes/Sprite Catalog", fileName = "ShapeSpriteCatalog")]
    public class ShapeSpriteCatalog : ScriptableObject
    {
        [SerializeField] private List<ShapeSpriteDefinition> shapes = new List<ShapeSpriteDefinition>();

        private Dictionary<string, Sprite> _spriteById;
        private List<string> _ids;

        public IReadOnlyList<string> GetAllIds()
        {
            if (_ids == null)
            {
                _ids = new List<string>();

                for (int i = 0; i < shapes.Count; i++)
                {
                    ShapeSpriteDefinition item = shapes[i];
                    if (item == null || string.IsNullOrWhiteSpace(item.Id) || item.Sprite == null) continue;

                    string id = item.Id.Trim();
                    if (!_ids.Contains(id))
                    {
                        _ids.Add(id);
                    }
                }
            }

            return _ids;
        }

        public Sprite GetSprite(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;

            if (_spriteById == null)
            {
                _spriteById = BuildDictionary();
            }

            _spriteById.TryGetValue(id.Trim(), out Sprite sprite);
            return sprite;
        }

        private Dictionary<string, Sprite> BuildDictionary()
        {
            Dictionary<string, Sprite> result = new Dictionary<string, Sprite>();

            for (int i = 0; i < shapes.Count; i++)
            {
                ShapeSpriteDefinition item = shapes[i];
                if (item == null || string.IsNullOrWhiteSpace(item.Id) || item.Sprite == null) continue;

                string id = item.Id.Trim();
                if (!result.ContainsKey(id))
                {
                    result.Add(id, item.Sprite);
                }
            }

            return result;
        }

        private void OnValidate()
        {
            _spriteById = null;
            _ids = null;
        }
    }
}
