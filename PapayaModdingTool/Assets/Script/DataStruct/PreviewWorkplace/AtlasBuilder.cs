using System.Collections.Generic;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.DataStruct.PreviewWorkplace
{
    public class AtlasBuilder
    {
        public static Texture2D CombineInRow(List<Texture2dWithCustomSize> textures, int gap = 4)
        {
            if (textures == null || textures.Count == 0)
                return null;

            // Find total width and max height (using custom sizes as box)
            int totalWidth = gap; // start gap
            int maxHeight = 0;

            foreach (var t in textures)
            {
                totalWidth += t.width + gap;
                if (t.height > maxHeight)
                    maxHeight = t.height;
            }

            // Create final texture
            Texture2D result = new Texture2D(totalWidth, maxHeight, TextureFormat.RGBA32, false);

            // Fill transparent
            Color32[] clear = new Color32[totalWidth * maxHeight];
            for (int i = 0; i < clear.Length; i++)
                clear[i] = new Color32(0, 0, 0, 0);
            result.SetPixels32(clear);

            // Blit each texture inside its custom box (crop if bigger, center if smaller)
            int cursorX = gap;
            foreach (var t in textures)
            {
                Texture2D tex = t.texture2D;

                int boxW = t.width;
                int boxH = t.height;

                int startX = cursorX;
                // int startY = (maxHeight - boxH) / 2; // center vertically in final canvas
                int startY = maxHeight - boxH;

                // Center inside the box
                int pasteX = startX + (boxW - tex.width) / 2;
                int pasteY = startY + (boxH - tex.height) / 2;

                // Clamp/crop if texture is larger than its box
                int copyW = Mathf.Min(tex.width, boxW);
                int copyH = Mathf.Min(tex.height, boxH);

                // Source start inside texture
                int srcX = tex.width > boxW ? (tex.width - boxW) / 2 : 0;
                int srcY = tex.height > boxH ? (tex.height - boxH) / 2 : 0;

                Color[] pixelsFloat = tex.GetPixels(srcX, srcY, copyW, copyH);
                Color32[] pixels = new Color32[pixelsFloat.Length];
                for (int i = 0; i < pixelsFloat.Length; i++)
                    pixels[i] = pixelsFloat[i];
                result.SetPixels32(pasteX + Mathf.Max(0, - (boxW - tex.width) / 2),
                                    pasteY + Mathf.Max(0, - (boxH - tex.height) / 2),
                                    copyW, copyH, pixels);

                cursorX += boxW + gap;
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