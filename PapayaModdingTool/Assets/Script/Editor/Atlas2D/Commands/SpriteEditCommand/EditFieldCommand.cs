using System;
using System.Collections.Generic;
using System.Diagnostics;
using PapayaModdingTool.Assets.Script.DataStruct.TextureData;
using PapayaModdingTool.Assets.Script.Program;

namespace PapayaModdingTool.Assets.Script.Editor.Atlas2D.Commands.SpriteEditCommand
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">Type of the field</typeparam>
    public class EditFieldCommand<T> : ICommand
    {
        private readonly T _newVal; // the new value to update to
        private readonly Func<SpriteButtonData> _GetCurr; // the current focusing data
        private readonly List<SpriteButtonData> _selected; // the selected datas
        private readonly Dictionary<SpriteButtonData, T> _oldVals = new(); // old values for the selected datas
        private readonly Action<T> _SetRenderVal; // set value of the render field
        private readonly Func<SpriteButtonData, T> _ReadDataVal; // get property of data
        private readonly Action<SpriteButtonData, T> _SetDataVal; // set property of data

        public EditFieldCommand(T newVal,
                                Func<SpriteButtonData> GetCurr,
                                List<SpriteButtonData> selected,
                                Action<T> SetRenderVal,
                                Func<SpriteButtonData, T> ReadDataVal,
                                Action<SpriteButtonData, T> SetDataVal)
        {
            _newVal = newVal;
            _GetCurr = GetCurr;
            _selected = selected;
            _SetDataVal = SetDataVal;
            _ReadDataVal = ReadDataVal;
            _SetRenderVal = SetRenderVal;
        }

        public void Execute()
        {
            _SetRenderVal(_newVal);
            foreach (SpriteButtonData data in _selected)
            {
                _oldVals[data] = _ReadDataVal(data);
                _SetDataVal(data, _newVal);
            }
        }

        public void Undo()
        {
            if (_oldVals.ContainsKey(_GetCurr()))
            {
                _SetRenderVal(_oldVals[_GetCurr()]);
            }

            foreach (SpriteButtonData data in _selected)
            {
                _SetDataVal(data, _oldVals[data]);
            }
        }
    }
}