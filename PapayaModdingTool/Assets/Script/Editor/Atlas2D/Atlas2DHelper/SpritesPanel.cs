using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AssetsTools.NET.Extra;
using PapayaModdingTool.Assets.Script.DataStruct.TextureData;
using PapayaModdingTool.Assets.Script.Editor.Atlas2D.Atlas2DMainHelper;
using PapayaModdingTool.Assets.Script.Editor.Atlas2D.Commands.SpriteEditCommand;
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
        public Func<ISpriteButtonDataListener> GetSpriteButtonDataListener;
        public Func<IFileFolderNameListener> GetFileFolderNameListener;
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
        private readonly Dictionary<SpriteButtonData, Texture2D> _scaledSpriteCache = new();

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
            var datas = GetDatas();
            if (GUILayout.Button(ELT("save_all_sprites")))
            {
                GetSaver().Save(GetJsonSavePath, _curr.sourcePath, datas);
                Debug.Log("Save success!");
            }
            EditorGUI.EndDisabledGroup();

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            DrawButtonGrid(datas, BUTTONS_PER_ROW, _bound.width / BUTTONS_PER_ROW - 15, 115, 5);

            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void DrawButtonGrid(List<SpriteButtonData> datas, int buttonsPerRow, float buttonWidth, float buttonHeight, float spacing)
        {
            if (datas == null || datas.Count == 0) return;

            float startX = 10f; // left margin
            float startY = 10f; // top margin
            float x = startX;
            float y = startY;

            int col = 0;

            for (int i = 0; i < datas.Count; i++)
            {
                Rect buttonRect = new(x, y, buttonWidth, buttonHeight);

                DrawImageButton(datas[i], buttonRect);

                col++;
                if (col >= buttonsPerRow)
                {
                    // Move to next row
                    col = 0;
                    x = startX;
                    y += buttonHeight + spacing;
                }
                else
                {
                    // Move to next column
                    x += buttonWidth + spacing;
                }
            }

            // Reserve space below grid for other controls
            GUILayout.Space(y + buttonHeight + startY);
        }

        private void DrawImageButton(SpriteButtonData data, Rect rect)
        {
            // Highlight selection
            if (data.isSelected)
                EditorGUI.DrawRect(rect, new(0.6f, 0.8f, 1f, 0.5f));

            if (!data.isInTrashbin)
            {
                // Draw overall background
                EditorGUI.DrawRect(rect, new(0.1f, 0.1f, 0.1f, 0.5f));
            }
            else
            {
                EditorGUI.DrawRect(rect, new(1f, 0f, 1f, 0.5f));
            }

            // Sprite size
            float spriteSize = 64f;

            // Draw sprite container background (light)
            Rect spriteRect = new(
                rect.x + (rect.width - spriteSize) / 2,
                rect.y + 5f,
                spriteSize,
                spriteSize
            );
            EditorGUI.DrawRect(spriteRect, new Color(0.9f, 0.9f, 0.9f, 0.1f));

            float spriteContainerSize = 64;
            Texture2D scaledSprite = GetScaledSprite(data, (int)spriteContainerSize);

            // Center in container
            Rect centeredRect = new Rect(
                spriteRect.x + (spriteContainerSize - scaledSprite.width) / 2,
                spriteRect.y + (spriteContainerSize - scaledSprite.height) / 2,
                scaledSprite.width,
                scaledSprite.height
            );

            // Draw texture
            GUI.DrawTexture(centeredRect, scaledSprite, ScaleMode.StretchToFill);

            // Button click area
            if (GUI.Button(spriteRect, GUIContent.none, GUIStyle.none))
            {
                GetBatchSelector().ClickSpriteButton(data,
                    SpritesBatchSelector.IsShiftHeld(),
                    SpritesBatchSelector.IsCtrlHeld());

                GetSpriteButtonDataListener()?.Update(data);
                GetFileFolderNameListener()?.Update(_curr.fileFolderName);
            }

            // Draw label below sprite
            float textY = spriteRect.yMax + 2f;

            string labelText = TruncateWithEllipsis(data.label, 8);
            GUIStyle labelStyle = EditorStyles.centeredGreyMiniLabel;
            Vector2 labelSize = labelStyle.CalcSize(new GUIContent(labelText));
            Rect labelRect = new(
                rect.x + (rect.width - labelSize.x) / 2,
                textY,
                labelSize.x,
                labelSize.y
            );
            GUI.Label(labelRect, labelText, labelStyle);

            // Draw coordinates
            string coord = (data.level < 0 || data.order < 0) ? "(null)" : $"({data.level}, {data.order})";
            GUIStyle coordStyle = (coord == "(null)") ? new GUIStyle(labelStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.red },
                fontSize = labelStyle.fontSize
            } : labelStyle;

            Vector2 coordSize = coordStyle.CalcSize(new GUIContent(coord));
            Rect coordRect = new(
                rect.x + (rect.width - rect.width) / 2,
                labelRect.yMax + 2f,
                rect.width,
                coordSize.y
            );
            GUI.Label(coordRect, $"{ELT("workplace")}: {coord}", coordStyle);

            // Draw pivot info
            string pivot = $"{ELT("pivot")}: <{Math.Round(data.pivot.x, 2)}, {Math.Round(data.pivot.y, 2)}>";
            Vector2 pivotSize = labelStyle.CalcSize(new GUIContent(pivot));
            Rect pivotRect = new(
                rect.x + (rect.width - pivotSize.x) / 2,
                coordRect.yMax + 2f,
                pivotSize.x,
                pivotSize.y
            );
            GUI.Label(pivotRect, pivot, labelStyle);
        }

        private Texture2D GetScaledSprite(SpriteButtonData data, int targetSize)
        {
            if (_scaledSpriteCache.TryGetValue(data, out Texture2D cached))
                return cached;

            float scale = Mathf.Min(targetSize / (float)data.sprite.width, targetSize / (float)data.sprite.height);
            int targetWidth = Mathf.RoundToInt(data.sprite.width * scale);
            int targetHeight = Mathf.RoundToInt(data.sprite.height * scale);

            Texture2D scaled = RescaleNearestNeighbor(data.sprite, targetWidth, targetHeight);
            _scaledSpriteCache[data] = scaled;
            return scaled;
        }

        private Texture2D RescaleNearestNeighbor(Texture2D source, int targetWidth, int targetHeight)
        {
            Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, false);

            for (int y = 0; y < targetHeight; y++)
            {
                for (int x = 0; x < targetWidth; x++)
                {
                    int srcX = Mathf.FloorToInt(x * (source.width / (float)targetWidth));
                    int srcY = Mathf.FloorToInt(y * (source.height / (float)targetHeight));
                    result.SetPixel(x, y, source.GetPixel(srcX, srcY));
                }
            }

            result.Apply();
            return result;
        }

        private string TruncateToEnd(string s, int len = 20)
        {
            return s.Length > len ? $"...{s[^len..]}" : s;
        }

        public static string TruncateWithEllipsis(string input, int keep = 10)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Only truncate if string is longer than what we want to keep
            if (input.Length <= keep * 2)
                return input;

            string start = input[..keep];
            string end   = input[^keep..];

            return $"{start}...{end}";
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
                    data.sprite = FlipSpriteCommand.FlipTextureByX(data.sprite);
                }
                if (data.hasFlipY)
                {
                    data.sprite = FlipSpriteCommand.FlipTextureByY(data.sprite);
                }
            }
        }
    }
}