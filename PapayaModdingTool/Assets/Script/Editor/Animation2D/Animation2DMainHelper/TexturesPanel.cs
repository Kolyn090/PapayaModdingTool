using System;
using UnityEditor;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.Animation2D.Animation2DMainHelper
{
    public class TexturesPanel
    {
        public Func<string, string> ELT;

        private Rect _bound;
        private bool _hasInit;
        private Vector2 _scrollPos;

        public void Initialize(Rect bound)
        {
            _bound = bound;
            _hasInit = true;
        }

        public void CreatePanel()
        {
            if (!_hasInit)
                return;

            // Draw background
            EditorGUI.DrawRect(_bound, new Color(0.2f, 0.2f, 0.2f));

            GUILayout.BeginArea(_bound);
            EditorGUILayout.LabelField("Found Textures", EditorStyles.boldLabel);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            

            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
        }
    }
}
