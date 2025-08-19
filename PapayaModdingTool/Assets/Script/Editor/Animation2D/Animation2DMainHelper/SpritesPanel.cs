using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AssetsTools.NET.Extra;
using PapayaModdingTool.Assets.Script.DataStruct.TextureData;
using PapayaModdingTool.Assets.Script.EventListener;
using PapayaModdingTool.Assets.Script.Reader.ImageDecoder;
using PapayaModdingTool.Assets.Script.Wrapper.TextureEncodeDecode;
using UnityEditor;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.Animation2DMainHelper
{
    public class SpritesPanel : ITexture2DButtonDataListener
    {
        private const int BUTTONS_PER_ROW = 4;

        public Func<string, string> ELT;
        public Func<AssetsManager> GetAssetsManager;
        public Func<TextureEncoderDecoder> GetTextureEncoderDecoder;
        public Func<ISpriteButtonDataListener> GetListener;

        private List<SpriteButtonData> _datas;
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
            if (_datas != null)
            {
                while (index < _datas.Count)
                {
                    EditorGUILayout.BeginHorizontal();

                    for (int col = 0; col < BUTTONS_PER_ROW && index < _datas.Count; col++, index++)
                    {
                        DrawImageButton(_datas[index]);
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
                // Debug.Log("Clicked: " + data.label);
                if (GetListener != null)
                {
                    GetListener()?.Update(data);
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            // Center the text horizontally
            GUILayout.Label(TruncateToEnd(data.label, 15), EditorStyles.centeredGreyMiniLabel, GUILayout.ExpandWidth(true));
            string coord;
            if (data.level < 0 || data.order < 0)
                coord = "<null>";
            else
                coord = $"({data.level}, {data.order})";

            GUIStyle redLabel = new(GUI.skin.label)
            {
                fontSize = EditorStyles.centeredGreyMiniLabel.fontSize,
                alignment = TextAnchor.MiddleCenter
            };
            redLabel.normal.textColor = Color.red;

            GUILayout.Label(coord,
                            coord == "<null>" ? redLabel : EditorStyles.centeredGreyMiniLabel,
                            GUILayout.ExpandWidth(true));

            GUILayout.EndVertical();
        }

        private string TruncateToEnd(string s, int len=20)
        {
            return s.Length > len ? $"...{s[^len..]}" : s;
        }

        public void Update(Texture2DButtonData data)
        {
            _datas = ImageReader.ReadSpriteButtonDatas(data.assetsInst,
                                                        GetAssetsManager(),
                                                        GetTextureEncoderDecoder());
            _datas = _datas.OrderBy(o =>
            {
                var match = Regex.Match(o.label, @"\d+$");
                if (match.Success && int.TryParse(match.Value, out int num))
                    return num;
                else
                    return int.MaxValue; // no number → push to end
            })
            .ThenBy(o => o.label) // optional: sort alphabetically among "no-number" names
            .ToList();
        }
    }
}