using System.Collections.Generic;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.DataStruct.PreviewWorkplace
{
    public class AtlasBuilder
    {
        public static Texture2D CombineInRow(List<Texture2D> textures, int gap = 4)
        {
            if (textures == null || textures.Count == 0)
                return null;

            // Find total width and max height
            int totalWidth = gap; // start gap
            int maxHeight = 0;

            foreach (Texture2D tex in textures)
            {
                totalWidth += tex.width + gap; // add tex + gap
                if (tex.height > maxHeight)
                    maxHeight = tex.height;
            }

            // Create final texture
            Texture2D result = new Texture2D(totalWidth, maxHeight, TextureFormat.RGBA32, false);

            // Fill transparent
            Color32[] clear = new Color32[totalWidth * maxHeight];
            for (int i = 0; i < clear.Length; i++)
                clear[i] = new Color32(0, 0, 0, 0);
            result.SetPixels32(clear);

            // Blit each texture
            int cursorX = gap;
            foreach (Texture2D tex in textures)
            {
                Color32[] pixels = tex.GetPixels32();

                // paste at bottom aligned (y = 0)
                result.SetPixels32(cursorX, 0, tex.width, tex.height, pixels);

                cursorX += tex.width + gap; // advance with gap
            }

            result.Apply(false);
            return result;
        }

        public static Texture2D CombineInColumn(List<Texture2D> textures, int gap = 4)
        {
            if (textures == null || textures.Count == 0)
                return null;

            // Find total height and max width
            int totalHeight = gap; // start gap
            int maxWidth = 0;

            foreach (Texture2D tex in textures)
            {
                totalHeight += tex.height + gap; // add tex + gap
                if (tex.width > maxWidth)
                    maxWidth = tex.width;
            }

            // Create final texture
            Texture2D result = new Texture2D(maxWidth, totalHeight, TextureFormat.RGBA32, false);

            // Fill with transparent pixels
            Color32[] clear = new Color32[maxWidth * totalHeight];
            for (int i = 0; i < clear.Length; i++)
                clear[i] = new Color32(0, 0, 0, 0);
            result.SetPixels32(clear);

            // Blit each texture
            int cursorY = gap;
            foreach (Texture2D tex in textures)
            {
                Color32[] pixels = tex.GetPixels32();

                // paste at left aligned (x = 0)
                result.SetPixels32(0, cursorY, tex.width, tex.height, pixels);

                cursorY += tex.height + gap; // advance with gap
            }

            result.Apply(false);
            return result;
        }
    }
}