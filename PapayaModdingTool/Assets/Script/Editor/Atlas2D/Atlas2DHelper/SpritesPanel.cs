using System;
using System.Collections.Generic;
using System.Linq;
using AssetsTools.NET.Extra;
using PapayaModdingTool.Assets.Script.DataStruct.TextureData;
using PapayaModdingTool.Assets.Script.Editor.Atlas2D.Atlas2DMainHelper;
using PapayaModdingTool.Assets.Script.Editor.Atlas2D.Commands.SpriteEditCommand;
using PapayaModdingTool.Assets.Script.Editor.Atlas2D.Shortcut;
using PapayaModdingTool.Assets.Script.EventListener;
using PapayaModdingTool.Assets.Script.Misc.ColorGen;
using PapayaModdingTool.Assets.Script.Misc.Paths;
using PapayaModdingTool.Assets.Script.Reader.Atlas2D;
using PapayaModdingTool.Assets.Script.Reader.ImageDecoder;
using PapayaModdingTool.Assets.Script.Wrapper.TextureEncodeDecode;
using PapayaModdingTool.Assets.Script.Writer.Atlas2D;
using UnityEditor;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.Atlas2DMainHelper
{
    public class SpritesPanel : ITexture2DButtonDataListener, IShortcutSavable, IShortcutNavigable
    {
        private const int BUTTONS_PER_ROW = 4;
        private const int BUTTON_HEIGHT = 115;
        private const int GAP = 5;

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
        public Func<ShortcutManager> GetShortcutManager;
        public Action<List<SpriteButtonData>> SetDatas;
        public Action<List<string>> SetAnimations;

        private Rect _bound;
        private bool _hasInit;
        private Vector2 _scrollPos;
        private Texture2DButtonData _currSelectedTextureData;
        private SpriteButtonData _currSelectedSpriteButtonData;
        private string GetJsonSavePath => string.Format(PredefinedPaths.Atlas2DSpritesPanelSaveJson,
                                                        GetProjectName(),
                                                        _currSelectedTextureData.fileFolderName);
        private readonly Dictionary<SpriteButtonData, Texture2D> _scaledSpriteCache = new();
        private readonly Dictionary<SpriteButtonData, bool> _spriteFlipX = new();
        private readonly Dictionary<SpriteButtonData, bool> _spriteFlipY = new();

        private static Texture2D _whiteTex;

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

            EditorGUI.BeginDisabledGroup(_currSelectedTextureData == null || string.IsNullOrWhiteSpace(_currSelectedTextureData.sourcePath));
            var datas = GetDatas();
            if (GUILayout.Button(ELT("save_all_sprites")))
            {
                GetSaver().Save(GetJsonSavePath, _currSelectedTextureData.sourcePath, datas);
                Debug.Log("Save success!");
            }
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button(ELT("sort_by_name")))
                {
                    SetDatas(SpriteButtonDataSorter.SortByLabel(GetDatas()));
                }
                if (GUILayout.Button(ELT("sort_by_workplace")))
                {
                    SetDatas(SpriteButtonDataSorter.SortByWorkplace(GetDatas()));
                }
            }
            GUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            DrawButtonGrid(datas, BUTTONS_PER_ROW,
                            _bound.width / BUTTONS_PER_ROW - (BUTTONS_PER_ROW-1)*GAP,
                            BUTTON_HEIGHT, GAP);

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
                GetFileFolderNameListener()?.Update(_currSelectedTextureData.fileFolderName);

                _currSelectedSpriteButtonData = data;
                FocusPanel();
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

            if (!string.IsNullOrWhiteSpace(data.animation))
            {
                DrawRotatedSquare(new(rect.x, rect.y, 10, 10), ColorGenerator.GetColorFromString(data.animation));
            }
        }

        private void DrawRotatedSquare(Rect rect, Color color, float angle = 45f)
        {
            // --- Lazy init of white texture ---
            if (_whiteTex == null)
            {
                _whiteTex = new Texture2D(1, 1);
                _whiteTex.SetPixel(0, 0, Color.white);
                _whiteTex.Apply();
            }

            // Save matrix
            Matrix4x4 oldMatrix = GUI.matrix;

            // Translate pivot to rect center, then rotate
            Vector2 pivot = rect.center;
            GUIUtility.RotateAroundPivot(angle, pivot);

            // Draw solid colored square
            Color oldColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, _whiteTex);
            GUI.color = oldColor;

            // Restore matrix
            GUI.matrix = oldMatrix;
        }

        private Texture2D GetScaledSprite(SpriteButtonData data, int targetSize)
        {
            if (_spriteFlipX.ContainsKey(data) && _spriteFlipX[data] == data.hasFlipX &&
                _spriteFlipY.ContainsKey(data) && _spriteFlipY[data] == data.hasFlipY &&
                _scaledSpriteCache.TryGetValue(data, out Texture2D cached))
                return cached;

            float scale = Mathf.Min(targetSize / (float)data.sprite.width, targetSize / (float)data.sprite.height);
            int targetWidth = Mathf.RoundToInt(data.sprite.width * scale);
            int targetHeight = Mathf.RoundToInt(data.sprite.height * scale);

            Texture2D scaled = RescaleNearestNeighbor(data.sprite, targetWidth, targetHeight);
            _scaledSpriteCache[data] = scaled;
            _spriteFlipX[data] = data.hasFlipX;
            _spriteFlipY[data] = data.hasFlipY;
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
            string end = input[^keep..];

            return $"{start}...{end}";
        }

        public void Update(Texture2DButtonData data)
        {
            static List<string> GetAnimations(List<SpriteButtonData> datas)
            {
                return datas.Select(x => x.animation).ToHashSet().ToList();
            }

            _currSelectedTextureData = data;
            if (data.IsStyle1)
            {
                List<SpriteButtonData> datas = ImageReader.ReadSpriteButtonDatas(data.assetsInst,
                                                            GetAssetsManager(),
                                                            GetTextureEncoderDecoder());
                LoadFromSave(data.sourcePath, datas);
                FlipTexture(datas);
                SetDatas(datas);
                SetDatas(SpriteButtonDataSorter.SortByLabel(GetDatas()));
                SetAnimations(GetAnimations(datas));
            }
            else if (data.IsStyle2)
            {
                List<SpriteButtonData> datas = ImageReader.ReadSpriteButtonDatas(data.importedTexturesPath);
                LoadFromSave(data.sourcePath, datas);
                FlipTexture(datas);
                SetDatas(datas);
                SetDatas(SpriteButtonDataSorter.SortByLabel(GetDatas()));
                SetAnimations(GetAnimations(datas));
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

        public void OnShortcutSave()
        {
            GetSaver().Save(GetJsonSavePath, _currSelectedTextureData.sourcePath, GetDatas());
            Debug.Log("Save success!");
        }

        public void OnShortNavigate(KeyCode keyCode)
        {
            if (_currSelectedSpriteButtonData == null || !_currSelectedSpriteButtonData.isSelected)
            {
                // don't do anything if the current editing sprite is not selected
                return;
            }

            // Get the coordinate of the current editing sprite
            (int, int) coord = GetCoordinate();

            (int, int) moveTo;
            if (keyCode == KeyCode.UpArrow)
            {
                moveTo = MoveUp(coord);
            }
            else if (keyCode == KeyCode.DownArrow)
            {
                moveTo = MoveDown(coord);
            }
            else if (keyCode == KeyCode.LeftArrow)
            {
                moveTo = MoveLeft(coord);
            }
            else if (keyCode == KeyCode.RightArrow)
            {
                moveTo = MoveRight(coord);
            }
            else
            {
                // ! Error: this shouldn't be possible
                moveTo = (-1, -1);
            }

            SpriteButtonData newSelected = GetDataInCoord(moveTo);
            GetBatchSelector().ClickSpriteButton(newSelected,
            SpritesBatchSelector.IsShiftHeld(),
            SpritesBatchSelector.IsCtrlHeld());

            GetSpriteButtonDataListener()?.Update(newSelected);
            GetFileFolderNameListener()?.Update(_currSelectedTextureData.fileFolderName);

            _currSelectedSpriteButtonData = newSelected;
            ScrollToSpriteCoord(moveTo, _bound.height - BUTTON_HEIGHT);
            FocusPanel();
        }

        private void ScrollToSpriteCoord((int, int) coord, float viewportHeight)
        {
            float elementHeight = BUTTON_HEIGHT + GAP;
            float elementTop = coord.Item1 * elementHeight;
            float elementBottom = elementTop + elementHeight;

            float viewTop = _scrollPos.y;
            float viewBottom = _scrollPos.y + viewportHeight;

            // If element is above the visible area → scroll up
            if (elementTop < viewTop)
            {
                _scrollPos.y = elementTop;
            }
            // If element is below the visible area → scroll down
            else if (elementBottom > viewBottom)
            {
                _scrollPos.y = elementBottom - viewportHeight;
            }
            // Else, already in view → do nothing
        }

        private SpriteButtonData GetDataInCoord((int, int) coord)
        {
            int index = coord.Item1 * BUTTONS_PER_ROW + coord.Item2;
            if (index < 0 || index >= GetDatas().Count)
                return null;
            return GetDatas()[index];
        }

        private (int, int) MoveUp((int, int) currPos)
        {
            int count = GetDatas().Count;
            int lastRow = (count - 1) / BUTTONS_PER_ROW;

            int newRow = currPos.Item1 > 0 ? currPos.Item1 - 1 : lastRow;

            // number of items in the new row
            int itemsInRow = (newRow == lastRow) 
                ? (count % BUTTONS_PER_ROW == 0 ? BUTTONS_PER_ROW : count % BUTTONS_PER_ROW)
                : BUTTONS_PER_ROW;

            int newCol = Math.Min(currPos.Item2, itemsInRow - 1);

            return (newRow, newCol);
        }

        private (int, int) MoveDown((int, int) currPos)
        {
            int count = GetDatas().Count;
            int lastRow = (count - 1) / BUTTONS_PER_ROW;

            int newRow = currPos.Item1 == lastRow ? 0 : currPos.Item1 + 1;

            // Clamp column if last row has fewer elements
            int maxColInRow = (newRow == lastRow) ? (count % BUTTONS_PER_ROW == 0 ? BUTTONS_PER_ROW : count % BUTTONS_PER_ROW) - 1
                                                : BUTTONS_PER_ROW - 1;

            int newCol = Math.Min(currPos.Item2, maxColInRow);

            return (newRow, newCol);
        }

        private (int, int) MoveLeft((int, int) currPos)
        {
            int count = GetDatas().Count;
            int lastRow = (count - 1) / BUTTONS_PER_ROW;

            if (currPos.Item2 > 0)
            {
                // just move left
                return (currPos.Item1, currPos.Item2 - 1);
            }
            else
            {
                // wrap to last column in this row
                int itemsInRow = (currPos.Item1 == lastRow)
                    ? (count % BUTTONS_PER_ROW == 0 ? BUTTONS_PER_ROW : count % BUTTONS_PER_ROW)
                    : BUTTONS_PER_ROW;

                return (currPos.Item1, itemsInRow - 1);
            }
        }

        private (int, int) MoveRight((int, int) currPos)
        {
            int count = GetDatas().Count;
            int lastRow = (count - 1) / BUTTONS_PER_ROW;

            // how many items in this row?
            int itemsInRow = (currPos.Item1 == lastRow) 
                ? (count % BUTTONS_PER_ROW == 0 ? BUTTONS_PER_ROW : count % BUTTONS_PER_ROW) 
                : BUTTONS_PER_ROW;

            if (currPos.Item2 < itemsInRow - 1)
                return (currPos.Item1, currPos.Item2 + 1);
            else
                return (currPos.Item1, 0); // wrap to start of the row
        }

        private (int, int) GetCoordinate()
        {
            int indexOfSelected = GetDatas().IndexOf(_currSelectedSpriteButtonData);

            if (indexOfSelected == -1)
            {
                // Shouldn't be happening
                return (-1, -1);
            }

            return (indexOfSelected / BUTTONS_PER_ROW, indexOfSelected % BUTTONS_PER_ROW);
        }

        private void FocusPanel()
        {
            GetShortcutManager().IsEnabled = true;
        }
    }
}