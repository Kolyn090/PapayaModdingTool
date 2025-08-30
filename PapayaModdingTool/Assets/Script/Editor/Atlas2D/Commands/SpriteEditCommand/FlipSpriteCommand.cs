using System;
using System.Collections.Generic;
using PapayaModdingTool.Assets.Script.DataStruct.TextureData;
using PapayaModdingTool.Assets.Script.Program;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.Atlas2D.Commands.SpriteEditCommand
{
    public enum FlipDirection
    {
        X,
        Y
    }

    public class FlipSpriteCommand : ICommand
    {
        private readonly FlipDirection _flipDirection;
        private readonly List<SpriteButtonData> _selected;
        private readonly Func<Texture2D> _GetDisplaySprite;
        private readonly Action<Texture2D> _SetDisplaySprite;
        public FlipSpriteCommand(FlipDirection flipDirection,
                                List<SpriteButtonData> selected,
                                Func<Texture2D> GetDisplaySprite,
                                Action<Texture2D> SetDisplaySprite)
        {
            _flipDirection = flipDirection;
            _selected = selected;
            _GetDisplaySprite = GetDisplaySprite;
            _SetDisplaySprite = SetDisplaySprite;
        }

        public void Execute()
        {
            foreach (SpriteButtonData data in _selected)
            {
                if (_flipDirection == FlipDirection.X)
                {
                    data.sprite = FlipTextureByX(data.sprite);
                    data.hasFlipX = !data.hasFlipX;
                }
                else
                {
                    data.sprite = FlipTextureByY(data.sprite);
                    data.hasFlipY = !data.hasFlipY;
                }
            }
            if (_flipDirection == FlipDirection.X)
            {
                _SetDisplaySprite(FlipTextureByX(_GetDisplaySprite()));
            }
            else
            {
                _SetDisplaySprite(FlipTextureByY(_GetDisplaySprite()));
            }
        }

        public void Undo()
        {
            Execute();
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