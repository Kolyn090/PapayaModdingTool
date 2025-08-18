using System;
using System.Collections.Generic;
using PapayaModdingTool.Assets.Script.DataStruct.TextureData;
using PapayaModdingTool.Assets.Script.EventListener;
using UnityEditor;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.Animation2DMainHelper
{
    public class SpritesPanel
    {
        private const int BUTTONS_PER_ROW = 4;

        public Func<string, string> ELT;
        public Func<List<SpriteButtonData>> GetSpriteButtonDatas;
        public Func<ISpriteButtonDataListener> GetListener;

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
            EditorGUILayout.LabelField("Found Sprites", EditorStyles.boldLabel);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            int index = 0;
            if (GetSpriteButtonDatas() != null)
            {
                while (index < GetSpriteButtonDatas().Count)
                {
                    EditorGUILayout.BeginHorizontal();

                    for (int col = 0; col < BUTTONS_PER_ROW && index < GetSpriteButtonDatas().Count; col++, index++)
                    {
                        DrawImageButton(GetSpriteButtonDatas()[index]);
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void DrawImageButton(SpriteButtonData data, params GUILayoutOption[] options)
        {
            GUILayout.BeginVertical("box", options);

            // Center the button horizontally
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(data.sprite, GUILayout.Width(64), GUILayout.Height(64)))
            {
                Debug.Log("Clicked: " + data.label);
                if (GetListener != null)
                {
                    GetListener()?.Update(data);
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            // Center the text horizontally
            GUILayout.Label(TruncateToEnd(data.label, 15), EditorStyles.centeredGreyMiniLabel, GUILayout.ExpandWidth(true));

            GUILayout.EndVertical();
        }

        private string TruncateToEnd(string s, int len=20)
        {
            return s.Length > len ? $"...{s[^len..]}" : s;
        }
    }
}