using System;
using System.Collections.Generic;
using PapayaModdingTool.Assets.Script.DataStruct.TextureData;
using PapayaModdingTool.Assets.Script.Program;

namespace PapayaModdingTool.Assets.Script.Editor.Atlas2D.Commands.SpriteEditCommand
{
    public class EditDropdownCommand : ICommand
    {
        private readonly Func<SpriteButtonData> _GetCurr; // the current focusing data
        private readonly int _newSelectedIndex; // only for curr
        private readonly Action<int> _SetIndex; // only for curr
        private readonly List<SpriteButtonData> _selected; // the selected datas
        private readonly Func<List<string>> _GetOptions;
        private readonly Action<string> _SetOption;
        private readonly Dictionary<SpriteButtonData, string> _oldVals = new();
        private readonly Func<SpriteButtonData, string> _ReadDataVal; // get property of data
        private readonly Action<SpriteButtonData, string> _SetDataVal; // set property of data

        public EditDropdownCommand(Func<SpriteButtonData> GetCurr,
                                    int newSelectedIndex,
                                    Action<int> SetIndex,
                                    List<SpriteButtonData> selected,
                                    Func<List<string>> GetOptions,
                                    Action<string> SetOption,
                                    Func<SpriteButtonData, string> ReadDataVal,
                                    Action<SpriteButtonData, string> SetDataVal)
        {
            _GetCurr = GetCurr;
            _newSelectedIndex = newSelectedIndex;
            _SetIndex = SetIndex;
            _selected = selected;
            _GetOptions = GetOptions;
            _SetOption = SetOption;
            _ReadDataVal = ReadDataVal;
            _SetDataVal = SetDataVal;
        }

        public void Execute()
        {
            if (_newSelectedIndex > 0)
            {
                _SetOption(_GetOptions()[_newSelectedIndex - 1]);
                foreach (SpriteButtonData data in _selected)
                {
                    _oldVals[data] = _ReadDataVal(data);
                    _SetDataVal(data, _GetOptions()[_newSelectedIndex]);
                }
            }
            else
            {
                _SetOption("");
                foreach (SpriteButtonData data in _selected)
                {
                    _oldVals[data] = _ReadDataVal(data);
                    _SetDataVal(data, "");
                }
            }
        }

        public void Undo()
        {
            if (_oldVals.ContainsKey(_GetCurr()))
            {
                _SetIndex(_GetOptions().IndexOf(_oldVals[_GetCurr()]));
            }

            foreach (SpriteButtonData data in _selected)
            {
                _SetDataVal(data, _oldVals[data]);
            }
        }
    }
}