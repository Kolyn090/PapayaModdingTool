using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AssetsTools.NET.Extra;
using PapayaModdingTool.Assets.Script.DataStruct.TextureData;
using PapayaModdingTool.Assets.Script.Editor.Atlas2D.Atlas2DMainHelper;
using PapayaModdingTool.Assets.Script.EventListener;
using PapayaModdingTool.Assets.Script.Misc.Paths;
using PapayaModdingTool.Assets.Script.Reader.Atlas2D;
using PapayaModdingTool.Assets.Script.Reader.ImageDecoder;
using PapayaModdingTool.Assets.Script.Wrapper.TextureEncodeDecode;
using PapayaModdingTool.Assets.Script.Writer.Atlas2D;
using UnityEditor;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.Atlas2DMainHelper
{
    public class SpritesPanel : ITexture2DButtonDataListener
    {
        private const int BUTTONS_PER_ROW = 4;

        public Func<string, string> ELT;
        public Func<AssetsManager> GetAssetsManager;
        public Func<TextureEncoderDecoder> GetTextureEncoderDecoder;
        public Func<ISpriteButtonDataListener> GetListener;
        public Func<List<SpriteButtonData>> GetDatas;
        public Func<SpritesBatchSelector> GetBatchSelector;
        public Func<SpritesPanelSaver> GetSaver;
        public Func<SpritesPanelReader> GetReader;
        public Func<string> GetProjectName;
        public Action<List<SpriteButtonData>> SetDatas;

        private Rect _bound;
        private bool _hasInit;
        private Vector2 _scrollPos;
        private Texture2DButtonData _curr;
        private string GetJsonSavePath => string.Format(PredefinedPaths.Atlas2DSpritesPanelSaveJson,
                                                        GetProjectName(),
                                                        _curr.fileFolderName);

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
            EditorGUILayout.LabelField(ELT("found_sprites"), EditorStyles.boldLabel);

            EditorGUI.BeginDisabledGroup(_curr == null || string.IsNullOrWhiteSpace(_curr.sourcePath));
            if (GUILayout.Button(ELT("save_all_sprites")))
            {
                GetSaver().Save(GetJsonSavePath, _curr.sourcePath, GetDatas());
            }
            EditorGUI.EndDisabledGroup();

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            int index = 0;
            if (GetDatas() != null)
            {
                while (index < GetDatas().Count)
                {
                    EditorGUILayout.BeginHorizontal();

                    for (int col = 0; col < BUTTONS_PER_ROW && index < GetDatas().Count; col++, index++)
                    {
                        DrawImageButton(GetDatas()[index]);
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void DrawImageButton(SpriteButtonData data, params GUILayoutOption[] options)
        {
            Rect areaRect = EditorGUILayout.BeginVertical("box");

            if (data.isSelected)
                EditorGUI.DrawRect(areaRect, new Color(0.6f, 0.8f, 1f, 1f));

            // Center the button horizontally
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(data.sprite, GUILayout.Width(64), GUILayout.Height(64)))
            {
                // Debug.Log("Clicked: " + data.label);
                GetBatchSelector().ClickSpriteButton(data, SpritesBatchSelector.IsShiftHeld(), SpritesBatchSelector.IsCtrlHeld());
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
                coord = "(null)";
            else
                coord = $"({data.level}, {data.order})";

            string pivot = $"{ELT("pivot")}: <{(float)Math.Round(data.pivot.x, 2)}, {(float)Math.Round(data.pivot.y, 2)}>";

            GUIStyle redLabel = new(GUI.skin.label)
            {
                fontSize = EditorStyles.centeredGreyMiniLabel.fontSize,
                alignment = TextAnchor.MiddleCenter
            };
            redLabel.normal.textColor = Color.red;

            GUILayout.Label($"{ELT("workplace")}: {coord}",
                            coord == "(null)" ? redLabel : EditorStyles.centeredGreyMiniLabel,
                            GUILayout.ExpandWidth(true));
            GUILayout.Label(pivot,
                            EditorStyles.centeredGreyMiniLabel,
                            GUILayout.ExpandWidth(true));

            EditorGUILayout.EndVertical();
        }

        private string TruncateToEnd(string s, int len = 20)
        {
            return s.Length > len ? $"...{s[^len..]}" : s;
        }

        public void Update(Texture2DButtonData data)
        {
            _curr = data;
            if (data.IsStyle1)
            {
                List<SpriteButtonData> datas = ImageReader.ReadSpriteButtonDatas(data.assetsInst,
                                                            GetAssetsManager(),
                                                            GetTextureEncoderDecoder());
                LoadFromSave(data.sourcePath, datas);
                FlipTexture(datas);
                SetDatas(datas);
                SetDatas(GetDatas().OrderBy(o =>
                {
                    var match = Regex.Match(o.originalLabel, @"\d+$");
                    if (match.Success && int.TryParse(match.Value, out int num))
                        return num;
                    else
                        return int.MaxValue; // no number → push to end
                })
                .ThenBy(o => o.originalLabel) // optional: sort alphabetically among "no-number" names
                .ToList());
            }
            else if (data.IsStyle2)
            {
                List<SpriteButtonData> datas = ImageReader.ReadSpriteButtonDatas(data.importedTexturesPath);
                LoadFromSave(data.sourcePath, datas);
                FlipTexture(datas);
                SetDatas(datas);
                SetDatas(GetDatas().OrderBy(o => o.originalLabel).ToList());
            }
            else
            {
                Debug.LogError("Invalid Texture2DButtonData. Abort.");
            }
        }

        private void LoadFromSave(string sourcePath, List<SpriteButtonData> datas)
        {
            GetReader().LoadDatas(datas, GetJsonSavePath, sourcePath);
        }

        private void FlipTexture(List<SpriteButtonData> datas)
        {
            foreach (SpriteButtonData data in datas)
            {
                if (data.hasFlipX)
                {
                    data.sprite = SpritesBatchOperator.FlipTextureByX(data.sprite);
                }
                if (data.hasFlipY)
                {
                    data.sprite = SpritesBatchOperator.FlipTextureByY(data.sprite);
                }
            }
        }
    }
}