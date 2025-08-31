using System.Collections.Generic;
using System.Linq;
using PapayaModdingTool.Assets.Script.DataStruct.TextureData;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.DataStruct.PreviewWorkplace
{
    public class Workplace
    {
        // Add horizontal padding so that we have space for level mark
        public static (Texture2D, List<PreviewMark>) CreatePreview(List<SpriteButtonData> workplace,
                                                                    int horizontalPadding,
                                                                    int gap = 1)
        {
            if (!IsWorkplaceValid(workplace))
            {
                Debug.LogWarning("Your workplace has conflict in level / order, please check!");
                Debug.LogWarning("No two sprites can have the same level and order.");
                Debug.LogWarning("Level and order must both be assigned (cannot be negative).");
                return (null, null);
            }

            Texture2D result = new(gap, gap);
            List<PreviewMark> marks = new();

            Dictionary<int, List<SpriteButtonData>> groups = GroupByLevel(workplace);

            List<Texture2D> rows = new();
            List<List<int>> allMidPointsX = new();
            foreach (int level in groups.Keys)
            {
                List<SpriteButtonData> group = groups[level];
                List<Texture2dWithCustomSize> sprites = group.Select(x => new Texture2dWithCustomSize()
                {
                    texture2D = x.sprite,
                    width = x.width,
                    height = x.height
                }).ToList();

                // Obtain Marks
                int x = 0;
                List<int> midPointsX = new();
                foreach (Texture2dWithCustomSize sprite in sprites)
                {
                    midPointsX.Add(x + gap + sprite.width / 2);
                    x += gap + sprite.width;
                }
                allMidPointsX.Add(midPointsX);
                // Obtain Marks

                Texture2D rowTexture = AtlasBuilder.CombineInRow(sprites, gap);
                rows.Add(rowTexture);
            }

            rows.Reverse();
            allMidPointsX.Reverse();

            // Obtain Marks
            List<int> allMidPointsY = new();
            int y = 0;
            foreach (Texture2D row in rows)
            {
                allMidPointsY.Add(y + gap + row.height / 2);
                y += gap + row.height;
            }

            for (int i = 0; i < allMidPointsY.Count; i++)
            {
                for (int j = 0; j < allMidPointsX[i].Count; j++)
                {
                    (int order, int level) = GetLevelOrderByIndex(groups, j, i);
                    if (j == 0)
                    {
                        marks.Add(new()
                        {
                            digits = level,
                            position = new(15, allMidPointsY[i]),
                            color = Color.red
                        });
                    }
                    marks.Add(new()
                    {
                        digits = order,
                        position = new(horizontalPadding + allMidPointsX[i][j], allMidPointsY[i])
                    });
                }
            }
            // Obtain Marks

            result = AtlasBuilder.CombineInColumn(rows, gap);

            if (result == null)
                return (null, null);

            result = AddHorizontalPadding(result, horizontalPadding);
            result.filterMode = FilterMode.Point;
            return (result, marks);
        }

        private static (int, int) GetLevelOrderByIndex(Dictionary<int, List<SpriteButtonData>> groups, int x, int y)
        {
            List<int> sortedKeys = groups.Keys.ToList(); // group by level (i.e. y)
            sortedKeys.Sort();
            sortedKeys.Reverse();
            int resultLevel = sortedKeys[y];
            int resultOrder = groups[resultLevel][x].order;
            return (resultOrder, resultLevel);
        }

        private static Texture2D AddHorizontalPadding(Texture2D original, int padding = 40)
        {
            int newWidth = original.width + padding * 2;
            int newHeight = original.height;

            // Create new texture
            Texture2D paddedTex = new Texture2D(newWidth, newHeight, original.format, false);

            // Fill with transparent
            Color[] clearPixels = new Color[newWidth * newHeight];
            for (int i = 0; i < clearPixels.Length; i++)
                clearPixels[i] = Color.clear;
            paddedTex.SetPixels(clearPixels);

            // Copy original texture into center
            for (int y = 0; y < original.height; y++)
            {
                for (int x = 0; x < original.width; x++)
                {
                    paddedTex.SetPixel(x + padding, y, original.GetPixel(x, y));
                }
            }

            paddedTex.Apply();
            return paddedTex;
        }

        public static Dictionary<int, List<SpriteButtonData>> GroupByLevel(List<SpriteButtonData> items)
        {
            return items
                .OrderBy(x => x.level)
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