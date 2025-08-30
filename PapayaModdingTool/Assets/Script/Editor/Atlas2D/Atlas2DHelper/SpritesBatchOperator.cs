using System;
using System.Collections.Generic;
using System.Linq;
using PapayaModdingTool.Assets.Script.DataStruct.TextureData;
using PapayaModdingTool.Assets.Script.Editor.Atlas2D.Commands.SpriteEditCommand;
using PapayaModdingTool.Assets.Script.Program;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.Atlas2D.Atlas2DMainHelper
{
    public class SpritesBatchOperator
    {
        public Func<List<SpriteButtonData>> GetDatas;
        public Func<CommandManager> GetCommandManager;
        public Func<Texture2D> GetDisplaySprite;
        public Action<Texture2D> SetDisplaySprite;

        public List<SpriteButtonData> Selected => GetDatas().Where(x => x.isSelected).ToList();

        public void RenameSpriteLabel(string newVal,
                                    Func<SpriteButtonData> GetCurr,
                                    Action<string> SetRenderVal)
        {
            GetCommandManager().ExecuteCommand(new EditFieldCommand<string>(
                newVal,
                GetCurr,
                Selected,
                SetRenderVal,
                data => data.label,
                (data, newVal) => data.label = newVal
            ));
        }

        public void ChangeLevelOfSelected(int newVal,
                                        Func<SpriteButtonData> GetCurr,
                                        Action<int> SetRenderVal)
        {
            GetCommandManager().ExecuteCommand(new EditFieldCommand<int>(
                newVal,
                GetCurr,
                Selected,
                SetRenderVal,
                data => data.level,
                (data, newVal) => data.level = newVal
            ));
        }

        public void ChangeWidthOfSelected(int newVal,
                                        Func<SpriteButtonData> GetCurr,
                                        Action<int> SetRenderVal)
        {
            GetCommandManager().ExecuteCommand(new EditFieldCommand<int>(
                newVal,
                GetCurr,
                Selected,
                SetRenderVal,
                data => data.width,
                (data, newVal) => data.width = newVal
            ));
        }

        public void ChangeHeightOfSelected(int newVal,
                                        Func<SpriteButtonData> GetCurr,
                                        Action<int> SetRenderVal)
        {
            GetCommandManager().ExecuteCommand(new EditFieldCommand<int>(
                newVal,
                GetCurr,
                Selected,
                SetRenderVal,
                data => data.height,
                (data, newVal) => data.height = newVal
            ));
        }

        public void ChangePivotXOfSelected(float newPivotX,
                                        Func<SpriteButtonData> GetCurr,
                                        Action<float> SetRenderVal)
        {
            GetCommandManager().ExecuteCommand(new EditFieldCommand<float>(
                newPivotX,
                GetCurr,
                Selected,
                SetRenderVal,
                data => data.pivot.x,
                (data, newVal) => data.pivot = new(newVal, data.pivot.y)
            ));
        }

        public void ChangePivotYOfSelected(float newPivotY,
                                        Func<SpriteButtonData> GetCurr,
                                        Action<float> SetRenderVal)
        {
            GetCommandManager().ExecuteCommand(new EditFieldCommand<float>(
                newPivotY,
                GetCurr,
                Selected,
                SetRenderVal,
                data => data.pivot.y,
                (data, newVal) => data.pivot = new(data.pivot.x, newVal)
            ));
        }

        public void AddPivotOfSelected(Func<SpriteButtonData> GetCurr,
                                        Action<float> SetRenderPivotX,
                                        Action<float> SetRenderPivotY,
                                        float addX = 0f, float addY = 0f)
        {
            if (addX != 0f)
            {
                GetCommandManager().ExecuteCommand(new EditFieldCommand<float>(
                    GetCurr().pivot.x + addX,
                    GetCurr,
                    Selected,
                    SetRenderPivotX,
                    data => data.pivot.x,
                    (data, newVal) => data.pivot = new(newVal, data.pivot.y)
                ));
            }
            if (addY != 0f)
            {
                GetCommandManager().ExecuteCommand(new EditFieldCommand<float>(
                    GetCurr().pivot.y + addY,
                    GetCurr,
                    Selected,
                    SetRenderPivotY,
                    data => data.pivot.y,
                    (data, newVal) => data.pivot = new(data.pivot.x, newVal)
                ));
            }
        }

        public void ChangeAnimationOfSelected(int newSelectedIndex,
                                            Func<SpriteButtonData> GetCurr,
                                            Action<int> SetIndex,
                                            Func<List<string>> GetOptions,
                                            Action<string> SetRenderAnimation)
        {
            GetCommandManager().ExecuteCommand(new EditDropdownCommand(
                GetCurr,
                newSelectedIndex,
                SetIndex,
                Selected,
                GetOptions,
                SetRenderAnimation,
                data => data.animation,
                (data, newAnimation) => data.animation = newAnimation
            ));
        }

        public void ChangeOrderOfSelected(int newVal,
                                        Func<SpriteButtonData> GetCurr,
                                        Action<int> SetRenderVal)
        {
            GetCommandManager().ExecuteCommand(new EditFieldCommand<int>(
                newVal,
                GetCurr,
                Selected,
                SetRenderVal,
                data => data.order,
                (data, newVal) => data.order = newVal
            ));
        }

        public void FlipXAllSelected()
        {
            GetCommandManager().ExecuteCommand(new FlipSpriteCommand(
                FlipDirection.X,
                Selected,
                GetDisplaySprite,
                SetDisplaySprite
            ));
        }

        public void FlipYAllSelected()
        {
            GetCommandManager().ExecuteCommand(new FlipSpriteCommand(
                FlipDirection.Y,
                Selected,
                GetDisplaySprite,
                SetDisplaySprite
            ));
        }

        public void AutoFillWorkplace()
        {
            // Add all valid pairs
            HashSet<(int, int)> pairs = new();
            foreach (SpriteButtonData data in GetDatas())
            {
                if (data.level >= 0 && data.order >= 0)
                    pairs.Add((data.level, data.order));
            }

            // For all selected sprites, auto generate level & order for them
            // If the sprite is already valid, skip

            int currLevel = 0;
            foreach (SpriteButtonData data in GetDatas())
            {
                if (data.isSelected && (data.level < 0 || data.order < 0))
                {
                    (int level, int order) = FindSmallestMissingPair(pairs, currLevel);
                    data.level = level;
                    data.order = order;
                    pairs.Add((level, order));
                }
                currLevel = data.level;
            }
        }

        private static (int, int) FindSmallestMissingPair(HashSet<(int, int)> pairs, int x)
        {
            if (x < 0)
            {
                x = 0;
            }

            for (int y = 0; ; y++)  // loop forever over y
            {
                if (!pairs.Contains((x, y)))
                    return (x, y);
            }
        }
    }
}