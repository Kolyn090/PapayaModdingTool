using System;
using System.Collections.Generic;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.Atlas2D.Atlas2DHelper
{
    public class Mark
    {
        public string text = "Text";
        public Vector2 position = Vector2.zero;
        public Color color = Color.black;
        public int fontSize = 40;
    }

    public class PreviewMarkPanel
    {
        private Texture2D _workplaceTexture;
        private RenderTexture _renderTexture;

        private Vector2 _panOffset = Vector2.zero;
        private float _zoom = 1f;
        private Rect _imageRect;

        #region Optimization
        private Vector2 _lastPanOffset;
        private float _lastZoom;
        private Vector2Int _lastRenderSize;

        private bool _isPanning = false;
        private Vector2 _lastMousePos;
        #endregion

        public PreviewMarkPanel(Rect imageRect)
        {
            _imageRect = imageRect;
        }

        // Call this when the parent workplace texture is updated
        public void MakeWorkplaceTexture(List<Mark> marks,
                                        int workplaceTextureWidth,
                                        int workplaceTextureHeight)
        {

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
            blitMat.SetFloat("_Zoom", _zoom);
            blitMat.SetVector("_PanOffset", new Vector2(_panOffset.x / _workplaceTexture.width, _panOffset.y / _workplaceTexture.height));

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
    }
}