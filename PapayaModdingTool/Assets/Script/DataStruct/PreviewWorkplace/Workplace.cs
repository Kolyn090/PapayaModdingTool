using System.Collections.Generic;
using System.Linq;
using PapayaModdingTool.Assets.Script.DataStruct.TextureData;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.DataStruct.PreviewWorkplace
{
    public class Workplace
    {
        public static Texture2D CreatePreview(List<SpriteButtonData> workplace, int gap = 1)
        {
            if (!IsWorkplaceValid(workplace))
            {
                Debug.LogWarning("Your workplace has conflict in level / order, please check!");
                Debug.LogWarning("No two sprites can have the same level and order.");
                Debug.LogWarning("Level and order must both be assigned (cannot be negative).");
                return null;
            }

            Texture2D result = new(gap, gap);

            Dictionary<int, List<SpriteButtonData>> groups = GroupByLevel(workplace);

            List<Texture2D> rows = new();
            foreach (int level in groups.Keys)
            {
                List<SpriteButtonData> group = groups[level];
                List<Texture2dWithCustomSize> sprites = group.Select(x => new Texture2dWithCustomSize()
                {
                    texture2D = x.sprite,
                    width = x.width,
                    height = x.height
                }).ToList();
                Texture2D rowTexture = AtlasBuilder.CombineInRow(sprites, gap);
                rows.Add(rowTexture);
            }

            rows.Reverse();
            result = AtlasBuilder.CombineInColumn(rows, gap);

            result.filterMode = FilterMode.Point;
            return result;
        }

        public static Dictionary<int, List<SpriteButtonData>> GroupByLevel(List<SpriteButtonData> items)
        {
            return items
                .GroupBy(x => x.level)
                .ToDictionary(g => g.Key, g => g.OrderBy(o => o.order).ToList());
        }

        // In a texture, cannot have two sprites with the same
        // level & order
        public static bool IsWorkplaceValid(List<SpriteButtonData> workplace)
        {
            HashSet<(int, int)> seen = new();
            foreach (SpriteButtonData spriteButtonData in workplace)
            {
                // Debug.Log(spriteButtonData.level);
                // Debug.Log(spriteButtonData.order);
                if (spriteButtonData.level < 0 || spriteButtonData.order < 0)
                    return false;
                (int, int) key = (spriteButtonData.level, spriteButtonData.order);
                if (!seen.Add(key))
                    return false;
            }
            return true;
        }
    }
}