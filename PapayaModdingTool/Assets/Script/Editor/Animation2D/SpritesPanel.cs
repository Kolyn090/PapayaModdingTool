using System;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.Animation2D
{
    public class SpritesPanel
    {
        public Func<string, string> ELT;
        private Rect _bound;
        private bool _hasInit;

        public void Initialize(Rect bound)
        {
            _bound = bound;
            _hasInit = true;
        }

        public void CreatePanel()
        {
            if (!_hasInit)
                return;
            
            
        }
    }
}