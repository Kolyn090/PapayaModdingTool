using System;
using System.Collections.Generic;
using System.Linq;
using PapayaModdingTool.Assets.Script.DataStruct.TextureData;
using UnityEditor;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.Animation2DMainHelper
{
    public class PreviewWorkPlace
    {
        public static Texture2D CreatePreview(List<SpriteButtonData> workplace, int gap=1)
        {
            if (!IsWorkplaceValid(workplace))
            {
                Debug.LogWarning("Your workplace has conflict in level / order, please check!");
                Debug.LogWarning("No two sprites can have the same level and order.");
                Debug.LogWarning("Level and order must both be assigned (cannot be negative).");
                return null;
            }

            Texture2D result = new(gap, gap);

            // Sort the work place by level.
            // Within each level, sort by order.
            workplace = workplace.OrderBy(o => o.level).ThenBy(o => o.order).ToList();

            // Start = top-left
            // End = bottom-right
            Dictionary<int, (int, int)> levelStartTracker = new();
            Dictionary<int, (int, int)> levelEndTracker = new();
            List<int> orderedLevel = new(); // tracks order as added

            (int, int) lastEndPoint = (0, 0);
            foreach (SpriteButtonData sprite in workplace)
            {
                int currLevel = sprite.level;
                if (orderedLevel.Contains(currLevel))
                {
                    Debug.Log($"Level {currLevel} exists");
                    // Get the place point for sprite
                    int placeY = levelStartTracker[currLevel].Item2 - gap;
                    int placeX = levelEndTracker[currLevel].Item1 + gap;

                    result = BlitBelow(result, sprite.sprite, placeX, placeY, Mathf.Abs(-placeY - result.height));
                    Debug.Log($"Placed {sprite.label} at ({placeX}, {placeY})");

                    // Update end
                    (int endX, int endY) = levelEndTracker[currLevel];
                    int startY = levelStartTracker[currLevel].Item2;
                    levelEndTracker[currLevel] = (placeX + sprite.width, endY < startY + sprite.height ? -endY : startY - sprite.height);
                    lastEndPoint = levelEndTracker[currLevel];
                    Debug.Log($"Update end {currLevel} at ({lastEndPoint.Item1}, {lastEndPoint.Item2})");
                }
                else
                {
                    Debug.Log($"Level {currLevel} doesn't exist");
                    // Create a new level
                    levelStartTracker[currLevel] = (gap, lastEndPoint.Item2);
                    orderedLevel.Add(currLevel);

                    int placeY = levelStartTracker[currLevel].Item2 - gap;
                    int placeX = gap;

                    result = BlitBelow(result, sprite.sprite, placeX, placeY, Mathf.Abs(-placeY - result.height));
                    Debug.Log($"Placed {sprite.label} at ({placeX}, {placeY})");

                    // Update end
                    if (levelEndTracker.ContainsKey(currLevel))
                    {
                        (int endX, int endY) = levelEndTracker[currLevel];
                        int startY = levelStartTracker[currLevel].Item2;
                        levelEndTracker[currLevel] = (placeX + sprite.width, endY < startY + sprite.height ? -endY : startY - sprite.height);
                        lastEndPoint = levelEndTracker[currLevel];
                        Debug.Log($"Update end {currLevel} at ({lastEndPoint.Item1}, {lastEndPoint.Item2})");
                    }
                    else
                    {
                        int startY = levelStartTracker[currLevel].Item2;
                        levelEndTracker[currLevel] = (placeX + sprite.width, startY - sprite.height);
                        lastEndPoint = levelEndTracker[currLevel];
                        Debug.Log($"Update end {currLevel} at ({lastEndPoint.Item1}, {lastEndPoint.Item2})");
                    }
                }
            }

            result.filterMode = FilterMode.Point;
            return result;
        }

        public static Texture2D Blit(Texture2D addTo, Texture2D sprite, int x, int y)
        {
            int newWidth  = Mathf.Max(addTo.width,  Mathf.Abs(x) + sprite.width);
            int newHeight = Mathf.Max(addTo.height, Mathf.Abs(y) + sprite.height);

            // Create new texture (always RGBA32 for safety)
            Texture2D result = new(newWidth, newHeight, TextureFormat.RGBA32, false);

            // Fill with transparent pixels
            Color32[] clear = new Color32[newWidth * newHeight];
            for (int i = 0; i < clear.Length; i++) clear[i] = new Color32(0, 0, 0, 0);
            result.SetPixels32(clear);

            // Copy AddTo
            Color32[] addToPixels = addTo.GetPixels32();
            result.SetPixels32(0, 0, addTo.width, addTo.height, addToPixels);

            // Copy sprite at (x, y)
            Color32[] spritePixels = sprite.GetPixels32();
            result.SetPixels32(x, y, sprite.width, sprite.height, spritePixels);

            result.Apply(false);
            return result;
        }

        public static Texture2D BlitBelow(Texture2D addTo, Texture2D sprite, int x, int y, int offsetY=0)
        {
            // If y is negative, shift everything up so y >= 0
            if (y < 0)
            {
                // offsetY = -y;   // how much we need to shift
                y = 0;
            }

            int newWidth  = Mathf.Max(addTo.width,  x + sprite.width);
            int newHeight = Mathf.Max(addTo.height + offsetY, y + sprite.height);

            Texture2D result = new Texture2D(newWidth, newHeight, TextureFormat.RGBA32, false);

            // Fill with transparent
            Color32[] clear = new Color32[newWidth * newHeight];
            for (int i = 0; i < clear.Length; i++) clear[i] = new Color32(0, 0, 0, 0);
            result.SetPixels32(clear);

            // Copy addTo shifted up if needed
            Color32[] addToPixels = addTo.GetPixels32();
            result.SetPixels32(0, offsetY, addTo.width, addTo.height, addToPixels);

            // Copy sprite at (x, y)
            Color32[] spritePixels = sprite.GetPixels32();
            result.SetPixels32(x, y, sprite.width, sprite.height, spritePixels);

            result.Apply(false);
            return result;
        }

        // In a texture, cannot have two sprites with the same
        // level & order
        public static bool IsWorkplaceValid(List<SpriteButtonData> workplace)
        {
            HashSet<(int, int)> seen = new();
            foreach (SpriteButtonData spriteButtonData in workplace)
            {
                // Debug.Log(spriteButtonData.level);
                // Debug.Log(spriteButtonData.order);
                if (spriteButtonData.level < 0 || spriteButtonData.order < 0)
                    return false;
                (int, int) key = (spriteButtonData.level, spriteButtonData.order);
                if (!seen.Add(key))
                    return false;
            }
            return true;
        }
    }

    public class PreviewTexturePanel
    {
        private const float ZOOM_MIN = 1f;
        private const float ZOOM_MAX = 4f;

        public Func<string, string> ELT;

        private Rect _guiRect; 
        private Rect _imageRect;

        private bool _hasInit;

        private Texture2D _workplaceTexture;
        private Texture2D _renderTexture;
        private Texture2D _checkerBoardTexture;
        private Vector2 _panOffset = Vector2.zero;
        private float _zoom = 1f;
        private List<SpriteButtonData> _workplace;
        private bool _needUpdateWorkplaceTexture = false;

        public void Initialize(Rect bound)
        {
            float totalHeight = bound.height;
            float guiHeight = totalHeight * 0.1f;
            float imageHeight = totalHeight - guiHeight;
            _guiRect = new Rect(bound.x, bound.y, bound.width, guiHeight);
            _imageRect = new Rect(bound.x, bound.y + guiHeight, bound.width, imageHeight);

            // GetTexture().filterMode = FilterMode.Point;

            _checkerBoardTexture = new(2, 2)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Repeat
            };
            _checkerBoardTexture.SetPixels32(new Color32[]
            {
                new(153,153,153,255), new(102,102,102,255),
                new(102,102,102,255), new(153,153,153,255)
            });
            _checkerBoardTexture.Apply();

            // _panOffset = new(0, -GetTexture().height / 2f);
            _hasInit = true;
        }

        public void UpdateWorkplace(List<SpriteButtonData> workplace)
        {
            _workplace = workplace;
            _needUpdateWorkplaceTexture = true;
        }

        public void CreatePanel()
        {
            if (!_hasInit)
                return;

            if (_needUpdateWorkplaceTexture && _workplace != null)
            {
                _workplaceTexture = PreviewWorkPlace.CreatePreview(_workplace);
                _needUpdateWorkplaceTexture = false;
            }

            GUI.BeginGroup(_guiRect);
            GUILayout.BeginHorizontal();

            // Set buttons to a fixed width
            EditorGUI.BeginDisabledGroup(_workplaceTexture == null);
            float buttonWidth = _guiRect.width * 0.5f - 10f;
            if (GUILayout.Button(ELT("zoom_in"), GUILayout.Width(buttonWidth)))
            {
                ZoomAtCenter(1.2f, _imageRect);
            }

            if (GUILayout.Button(ELT("zoom_out"), GUILayout.Width(buttonWidth)))
            {
                ZoomAtCenter(1 / 1.2f, _imageRect);
            }
            GUILayout.EndHorizontal();

            // Compute pan bounds based on zoomed image size
            float panWidth = Mathf.Max((_workplaceTexture != null ? _workplaceTexture.width : 0f) * _zoom - _imageRect.width, 0f) / 2f;
            float panHeight = Mathf.Max((_workplaceTexture != null ? _workplaceTexture.height : 0f) * _zoom - _imageRect.height, 0f) / 2f;
            float sliderWidth = _guiRect.width - 20f;
            _panOffset.x = EditorGUILayout.Slider(ELT("pan_x"), _panOffset.x, -panWidth, panWidth, GUILayout.Width(sliderWidth));
            _panOffset.y = EditorGUILayout.Slider(ELT("pan_y"), _panOffset.y, -panHeight, panHeight, GUILayout.Width(sliderWidth));
            EditorGUI.EndDisabledGroup();
            GUI.EndGroup();

            // --- Preview Panel ---
            GUI.Box(_imageRect, ELT("preview"));

            // Update and draw the preview texture
            GUI.BeginGroup(_imageRect);
            GUI.DrawTextureWithTexCoords(
                new Rect(0, 0, _imageRect.width, _imageRect.height),
                _checkerBoardTexture,
                new Rect(0, 0, _imageRect.width / 16f, _imageRect.height / 16f)
            );
            if (_workplaceTexture != null)
            {
                UpdatePreviewTexture((int)_imageRect.width, (int)_imageRect.height);
                GUI.DrawTexture(new Rect(0, 0, _imageRect.width, _imageRect.height), _renderTexture, ScaleMode.StretchToFill, true);
            }
            GUI.EndGroup();
        }

        public void SetPanOffset(Vector2 panOffset)
        {
            _panOffset = panOffset;
        }

        private void ZoomAtCenter(float zoomFactor, Rect previewRect)
        {
            float oldZoom = _zoom;
            _zoom = Mathf.Clamp(_zoom * zoomFactor, ZOOM_MIN, ZOOM_MAX);

            // Compute half sizes
            Vector2 halfPreview = new Vector2(previewRect.width, previewRect.height) * 0.5f;
            Vector2 halfImageOld = 0.5f * oldZoom * new Vector2(_workplaceTexture.width, _workplaceTexture.height);
            Vector2 halfImageNew = _zoom * 0.5f * new Vector2(_workplaceTexture.width, _workplaceTexture.height);

            // Compute old min/max pan
            float oldPanMinX = halfPreview.x - halfImageOld.x;
            float oldPanMaxX = halfImageOld.x - halfPreview.x;
            float oldPanMinY = halfPreview.y - halfImageOld.y;
            float oldPanMaxY = halfImageOld.y - halfPreview.y;

            // Compute new min/max pan
            float newPanMinX = halfPreview.x - halfImageNew.x;
            float newPanMaxX = halfImageNew.x - halfPreview.x;
            float newPanMinY = halfPreview.y - halfImageNew.y;
            float newPanMaxY = halfImageNew.y - halfPreview.y;

            // Adjust panOffset to preserve edge-relative position
            float tX = (_panOffset.x - oldPanMinX) / (oldPanMaxX - oldPanMinX);
            float tY = (_panOffset.y - oldPanMinY) / (oldPanMaxY - oldPanMinY);

            _panOffset.x = Mathf.Lerp(newPanMinX, newPanMaxX, tX);
            _panOffset.y = Mathf.Lerp(newPanMinY, newPanMaxY, tY);

            // Clamp again to be safe
            _panOffset.x = Mathf.Clamp(_panOffset.x, newPanMinX, newPanMaxX);
            _panOffset.y = Mathf.Clamp(_panOffset.y, newPanMinY, newPanMaxY);
        }

        private void UpdatePreviewTexture(int previewWidth, int previewHeight)
        {
            if (_renderTexture == null || _renderTexture.width != previewWidth || _renderTexture.height != previewHeight)
            {
                _renderTexture = new(previewWidth, previewHeight, TextureFormat.RGBA32, false)
                {
                    filterMode = FilterMode.Point
                };
            }

            Color[] srcPixels = _workplaceTexture.GetPixels();
            Color32[] dstPixels = new Color32[previewWidth * previewHeight];

            float invZoom = 1f / _zoom;
            float halfPreviewW = previewWidth * 0.5f;
            float halfPreviewH = previewHeight * 0.5f;
            float halfSrcW = _workplaceTexture.width * 0.5f;
            float halfSrcH = _workplaceTexture.height * 0.5f;

            for (int y = 0; y < previewHeight; y++)
            {
                float srcYf = (y - halfPreviewH) * invZoom + halfSrcH - _panOffset.y * invZoom;
                int srcYInt = Mathf.FloorToInt(srcYf);

                int dstRow = y * previewWidth;

                for (int x = 0; x < previewWidth; x++)
                {
                    float srcXf = (x - halfPreviewW) * invZoom + halfSrcW - _panOffset.x * invZoom;
                    int srcXInt = Mathf.FloorToInt(srcXf);

                    Color col = Color.clear;
                    if (srcXInt >= 0 && srcXInt < _workplaceTexture.width && srcYInt >= 0 && srcYInt < _workplaceTexture.height)
                        col = srcPixels[srcYInt * _workplaceTexture.width + srcXInt];

                    dstPixels[dstRow + x] = col;
                }
            }

            _renderTexture.SetPixels32(dstPixels);
            _renderTexture.Apply();
        }
    }
}