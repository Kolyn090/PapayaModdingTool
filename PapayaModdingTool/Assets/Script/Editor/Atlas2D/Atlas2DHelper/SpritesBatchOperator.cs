using System;
using System.Collections.Generic;
using System.Linq;
using PapayaModdingTool.Assets.Script.DataStruct.TextureData;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.Atlas2D.Atlas2DMainHelper
{
    public class SpritesBatchOperator
    {
        public Func<List<SpriteButtonData>> GetData;
        public Func<Texture2D> GetDisplaySprite;
        public Action<Texture2D> SetDisplaySprite;

        public void RenameSpriteLabel(string newLabel, SpriteButtonData curr)
        {
            // // Just change the last selected one
            // curr.label = newLabel;

            List<SpriteButtonData> selected = GetData().Where(x => x.isSelected).ToList();
            foreach (SpriteButtonData sprite in selected)
            {
                sprite.label = newLabel;
            }
        }

        public void ChangeLevelOfSelected(int newLevel)
        {
            List<SpriteButtonData> selected = GetData().Where(x => x.isSelected).ToList();
            foreach (SpriteButtonData sprite in selected)
            {
                sprite.level = newLevel;
            }
        }

        public void ChangeWidthOfSelected(int newWidth)
        {
            List<SpriteButtonData> selected = GetData().Where(x => x.isSelected).ToList();
            foreach (SpriteButtonData sprite in selected)
            {
                sprite.width = newWidth;
            }
        }

        public void ChangeHeightOfSelected(int newHeight)
        {
            List<SpriteButtonData> selected = GetData().Where(x => x.isSelected).ToList();
            foreach (SpriteButtonData sprite in selected)
            {
                sprite.height = newHeight;
            }
        }

        public void ChangePivotXOfSelected(float newPivotX)
        {
            List<SpriteButtonData> selected = GetData().Where(x => x.isSelected).ToList();
            foreach (SpriteButtonData sprite in selected)
            {
                sprite.pivot = new(newPivotX, sprite.pivot.y);
            }
        }

        public void ChangePivotYOfSelected(float newPivotY)
        {
            List<SpriteButtonData> selected = GetData().Where(x => x.isSelected).ToList();
            foreach (SpriteButtonData sprite in selected)
            {
                sprite.pivot = new(sprite.pivot.x, newPivotY);
            }
        }

        public void ChangeAnimationOfSelected(string newAnimation)
        {
            List<SpriteButtonData> selected = GetData().Where(x => x.isSelected).ToList();
            foreach (SpriteButtonData sprite in selected)
            {
                sprite.animation = newAnimation;
            }
        }

        public void ChangeOrderOfSelected(int newOrder)
        {
            List<SpriteButtonData> selected = GetData().Where(x => x.isSelected).ToList();
            foreach (SpriteButtonData sprite in selected)
            {
                sprite.order = newOrder;
            }
        }

        public void FlipXAllSelected()
        {
            List<SpriteButtonData> selected = GetData().Where(x => x.isSelected).ToList();
            FlipX(selected);
        }

        public void FlipYAllSelected()
        {
            List<SpriteButtonData> selected = GetData().Where(x => x.isSelected).ToList();
            FlipY(selected);
        }

        public void FlipX(List<SpriteButtonData> sprites)
        {
            foreach (SpriteButtonData sprite in sprites)
            {
                sprite.sprite = FlipTextureByX(sprite.sprite);
                sprite.hasFlipX = !sprite.hasFlipX;
            }
            SetDisplaySprite(FlipTextureByX(GetDisplaySprite()));
        }

        public static Texture2D FlipTextureByX(Texture2D original)
        {
            int width = original.width;
            int height = original.height;

            Texture2D flipped = new(width, height, original.format, false);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Mirror vertically: pixel from (x, y) goes to (x, height - 1 - y)
                    flipped.SetPixel(x, height - 1 - y, original.GetPixel(x, y));
                }
            }

            flipped.Apply();
            return flipped;
        }

        public void FlipY(List<SpriteButtonData> sprites)
        {
            foreach (SpriteButtonData sprite in sprites)
            {
                sprite.sprite = FlipTextureByY(sprite.sprite);
                sprite.hasFlipY = !sprite.hasFlipY;
            }
            SetDisplaySprite(FlipTextureByY(GetDisplaySprite()));
        }

        public static Texture2D FlipTextureByY(Texture2D original)
        {
            int width = original.width;
            int height = original.height;

            Texture2D flipped = new(width, height, original.format, false);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Mirror horizontally: pixel from (x, y) goes to (width - 1 - x, y)
                    flipped.SetPixel(width - 1 - x, y, original.GetPixel(x, y));
                }
            }

            flipped.Apply();
            return flipped;
        }
    }
}