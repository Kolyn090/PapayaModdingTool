using System.Collections.Generic;
using System.IO;
using PapayaModdingTool.Assets.Script.DataStruct.PreviewWorkplace;
using PapayaModdingTool.Assets.Script.Misc.Paths;
using UnityEditor;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.Atlas2D.Atlas2DHelper
{
    public class PreviewMarkPanel
    {
        private Texture2D _workplaceTexture;
        private RenderTexture _renderTexture;
        public RenderTexture RenderTexture => _renderTexture;

        private Rect _imageRect;
        private readonly ZoomPanController _zoomPanController;

        public PreviewMarkPanel(Rect imageRect, ZoomPanController zoomPanController)
        {
            _imageRect = imageRect;
            _zoomPanController = zoomPanController;
        }

        // Call this when the parent workplace texture is updated
        public void MakeWorkplaceTexture(List<PreviewMark> marks, int width, int height)
        {
            Texture2D tex = new(width, height, TextureFormat.RGBA32, false);
            Color[] clearPixels = new Color[width * height];
            for (int i = 0; i < clearPixels.Length; i++) clearPixels[i] = Color.clear;
            tex.SetPixels(clearPixels);

            // Load digit textures (0.png to 9.png)
            Texture2D[] digitTextures = new Texture2D[10];
            for (int i = 0; i <= 9; i++)
            {
                string path = Path.Combine(PredefinedPaths.DigitsPath, $"{i}.png");
                byte[] bytes = File.ReadAllBytes(path);
                Texture2D digitTex = new(2, 2, TextureFormat.RGBA32, false);
                digitTex.LoadImage(bytes);
                digitTextures[i] = digitTex;
            }

            foreach (var mark in marks)
            {
                string number = mark.digits.ToString();
                int totalWidth = 0;
                int maxHeight = 0;

                // Calculate total width and max height of the number
                foreach (char c in number)
                {
                    if (!char.IsDigit(c)) continue;
                    int digit = c - '0';
                    Texture2D digitTex = digitTextures[digit];
                    totalWidth += digitTex.width + 1; // +1 for gap
                    maxHeight = Mathf.Max(maxHeight, digitTex.height);
                }
                totalWidth -= 1; // Remove last extra gap

                // Compute top-left corner to start drawing so it's centered at mark.position
                int startX = Mathf.RoundToInt(mark.position.x - totalWidth * 0.5f);
                int startY = Mathf.RoundToInt(mark.position.y - maxHeight * 0.5f);

                int cursorX = startX;

                for (int d = 0; d < number.Length; d++)
                {
                    char c = number[d];
                    if (!char.IsDigit(c)) continue;

                    int digit = c - '0';
                    Texture2D digitTex = digitTextures[digit];
                    int digitWidth = digitTex.width;
                    int digitHeight = digitTex.height;

                    for (int y = 0; y < digitHeight; y++)
                    {
                        for (int x = 0; x < digitWidth; x++)
                        {
                            Color pixel = digitTex.GetPixel(x, y);
                            if (pixel.a > 0)
                            {
                                int px = cursorX + x;
                                int py = startY + y;
                                if (px >= 0 && px < width && py >= 0 && py < height)
                                    tex.SetPixel(px, py, mark.color);
                            }
                        }
                    }

                    cursorX += digitWidth + 1;
                }
            }

            tex.Apply();
            tex.filterMode = FilterMode.Point;
            _workplaceTexture = tex;
            // SaveTextureToAssets(_workplaceTexture, "debug_digits.png");
        }

        // Call this when the parent preview texture is updated
        public void UpdatePreviewTexture()
        {
            if (_renderTexture == null ||
                _renderTexture.width != _imageRect.width ||
                _renderTexture.height != _imageRect.height)
            {
                if (_renderTexture != null)
                    _renderTexture.Release();

                _renderTexture = new RenderTexture((int)_imageRect.width, (int)_imageRect.height, 0)
                {
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp
                };
            }

            // Make active before clearing/blitting
            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = _renderTexture;

            // Clear it
            GL.Clear(true, true, Color.clear);

            // Blit with shader
            Material blitMat = new Material(Shader.Find("Hidden/BlitZoomPan"));
            blitMat.SetTexture("_MainTex", _workplaceTexture);

            // Set zoom, pan, scale as before
            blitMat.SetFloat("_Zoom", _zoomPanController.Zoom);
            blitMat.SetVector("_PanOffset", new Vector2(_zoomPanController.PanOffset.x / _workplaceTexture.width,
                                                        _zoomPanController.PanOffset.y / _workplaceTexture.height));

            float panelAspect = _imageRect.width / _imageRect.height;
            float textureAspect = (float)_workplaceTexture.width / _workplaceTexture.height;
            Vector2 scale = Vector2.one;
            if (textureAspect > panelAspect) scale.y = panelAspect / textureAspect;
            else scale.x = textureAspect / panelAspect;
            blitMat.SetVector("_Scale", scale);

            Graphics.Blit(_workplaceTexture, _renderTexture, blitMat);

            // Restore previous RenderTexture
            RenderTexture.active = prev;
        }

        // ! Debug
        private static void SaveTextureToAssets(Texture2D tex, string assetPath)
        {
            if (tex == null)
            {
                Debug.LogError("SaveTextureToAssets: Texture is null!");
                return;
            }

            // Encode to PNG
            byte[] pngData = tex.EncodeToPNG();
            if (pngData == null)
            {
                Debug.LogError("SaveTextureToAssets: Failed to encode texture!");
                return;
            }

            // Ensure path starts with Assets/
            if (!assetPath.StartsWith("Assets/"))
                assetPath = "Assets/" + assetPath;

            // Write file
            File.WriteAllBytes(assetPath, pngData);

            // Refresh so Unity picks it up
            AssetDatabase.ImportAsset(assetPath);
            AssetDatabase.Refresh();

            Debug.Log($"Saved texture to {assetPath}");
        }
    }
}