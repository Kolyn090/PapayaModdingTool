using System;
using System.Collections.Generic;
using PapayaModdingTool.Assets.Script.DataStruct.TextureData;
using PapayaModdingTool.Assets.Script.Program;

namespace PapayaModdingTool.Assets.Script.Editor.Atlas2D.Commands.SpriteEditCommand
{
    public class AutoFillWorkplaceCommand : ICommand
    {
        private readonly Func<List<SpriteButtonData>> _GetDatas;
        private readonly Dictionary<SpriteButtonData, (int, int)> _oldWorkplace = new();

        public AutoFillWorkplaceCommand(Func<List<SpriteButtonData>> GetDatas)
        {
            _GetDatas = GetDatas;
        }

        public void Execute()
        {
            // Add all valid pairs
            HashSet<(int, int)> pairs = new();
            foreach (SpriteButtonData data in _GetDatas())
            {
                if (data.level >= 0 && data.order >= 0)
                    pairs.Add((data.level, data.order));
            }

            // For all selected sprites, auto generate level & order for them
            // If the sprite is already valid, skip
            int currLevel = 0;
            foreach (SpriteButtonData data in _GetDatas())
            {
                if (data.isSelected && (data.level < 0 || data.order < 0))
                {
                    // Record the old workplace values
                    _oldWorkplace[data] = (data.level, data.order);

                    (int level, int order) = FindSmallestMissingPair(pairs, currLevel);
                    data.level = level;
                    data.order = order;
                    pairs.Add((level, order));
                }
                currLevel = data.level;
            }
        }

        public void Undo()
        {
            foreach (SpriteButtonData data in _oldWorkplace.Keys)
            {
                (int level, int order) = _oldWorkplace[data];
                data.level = level;
                data.order = order;
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