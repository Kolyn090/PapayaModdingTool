using System;
using System.Collections.Generic;
using PapayaModdingTool.Assets.Script.DataStruct.TextureData;
using UnityEditor;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.Animation2D
{
    public class SpritesPanel
    {
        public Func<string, string> ELT;
        public Func<List<SpriteButtonData>> GetSpriteButtonDatas;
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
            EditorGUI.DrawRect(_bound, new Color(0.2f, 0.25f, 0.3f));

            GUILayout.BeginArea(_bound);
            {
                EditorGUILayout.LabelField("Scrollable Button Grid", EditorStyles.boldLabel);

                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

                int buttonsPerRow = 6; // how many per row
                int totalButtons = 130;
                int index = 0;

                // while (index < totalButtons)
                // {
                //     EditorGUILayout.BeginHorizontal();

                //     for (int col = 0; col < buttonsPerRow && index < totalButtons; col++, index++)
                //     {
                //         DrawImageButton(new ButtonData
                //         {
                //             icon = EditorGUIUtility.IconContent("console.infoicon").image,
                //             label = "Button " + index
                //         }, GUILayout.Width(80));
                //     }

                //     EditorGUILayout.EndHorizontal();
                // }

                EditorGUILayout.EndScrollView();
            }
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
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            // Center the text horizontally
            GUILayout.Label(data.label, EditorStyles.centeredGreyMiniLabel, GUILayout.ExpandWidth(true));

            GUILayout.EndVertical();
        }
    }
}