using System;
using System.Collections.Generic;
using PapayaModdingTool.Assets.Script.DataStruct.TextureData;
using PapayaModdingTool.Assets.Script.EventListener;
using UnityEditor;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.Atlas2D.Atlas2DMainHelper
{
    public class TexturesPanel
    {
        private const int BUTTONS_PER_ROW = 1;

        private readonly Func<string, string> _ELT;
        private readonly List<Texture2DButtonData> _texture2DButtonDatas;
        private readonly ITexture2DButtonDataListener _listener;

        private Rect _bound;
        private bool _hasInit;
        private Vector2 _scrollPos;

        public TexturesPanel(Func<string, string> ELT,
                            List<Texture2DButtonData> texture2DButtonDatas,
                            ITexture2DButtonDataListener listener)
        {
            _ELT = ELT;
            _texture2DButtonDatas = texture2DButtonDatas;
            _listener = listener;
        }

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
            EditorGUILayout.LabelField(_ELT("found_textures"), EditorStyles.boldLabel);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            int index = 0;
            if (_texture2DButtonDatas != null)
            {
                while (index < _texture2DButtonDatas.Count)
                {
                    EditorGUILayout.BeginHorizontal();

                    for (int col = 0; col < BUTTONS_PER_ROW && index < _texture2DButtonDatas.Count; col++, index++)
                    {
                        DrawImageButton(_texture2DButtonDatas[index]);
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void DrawImageButton(Texture2DButtonData data, params GUILayoutOption[] options)
        {
            GUILayout.BeginVertical("box", options);

            // Center the button horizontally
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (data.IsStyle1)
            {
                if (GUILayout.Button(data.texture, GUILayout.Width(64), GUILayout.Height(64)))
                {
                    if (_listener != null)
                    {
                        _listener?.Update(data);
                    }
                }
            }
            else if (data.IsStyle2)
            {
                if (GUILayout.Button(EditorGUIUtility.IconContent("Folder Icon").image as Texture2D, GUILayout.Width(64), GUILayout.Height(64)))
                {
                    if (_listener != null)
                    {
                        _listener?.Update(data);
                    }
                }
            }
            else
            {
                // Invalid button. Don't do anything
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            // Center the text horizontally
                GUILayout.Label(TruncateToEnd(data.label, 35), EditorStyles.centeredGreyMiniLabel, GUILayout.ExpandWidth(true));

            GUILayout.EndVertical();
        }

        private string TruncateToEnd(string s, int len=20)
        {
            return s.Length > len ? $"...{s[^len..]}" : s;
        }
    }
}
