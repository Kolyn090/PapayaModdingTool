using System;
using System.Collections.Generic;
using PapayaModdingTool.Assets.Script.DataStruct.PreviewWorkplace;
using PapayaModdingTool.Assets.Script.DataStruct.TextureData;
using PapayaModdingTool.Assets.Script.Writer.Atlas2D;
using UnityEditor;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.Atlas2DMainHelper
{
    public class PreviewTexturePanel
    {
        private const float ZOOM_MIN = 0.3f;
        private const float ZOOM_MAX = 4f;

        public Func<string, string> ELT;
        public Func<WorkplaceExportor> GetWorkplaceExportor;
        public Func<List<SpriteButtonData>> GetDatas;

        private Rect _guiRect; 
        private Rect _imageRect;

        private bool _hasInit;

        private Texture2D _workplaceTexture;
        private RenderTexture  _renderTexture;
        private Texture2D _checkerBoardTexture;
        private Vector2 _panOffset = Vector2.zero;
        private float _zoom = 1f;
        private List<SpriteButtonData> _workplace;
        private bool _needUpdateWorkplaceTexture = false;

#region Optimization
        
#endregion

        public void Initialize(Rect bound)
        {
            float totalHeight = bound.height;
            float guiHeight = totalHeight * 0.155f;
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
                _workplaceTexture = Workplace.CreatePreview(_workplace);
                _needUpdateWorkplaceTexture = false;
            }

            GUI.BeginGroup(_guiRect);
            EditorGUILayout.LabelField(ELT("workplace"), EditorStyles.boldLabel);

            EditorGUI.BeginDisabledGroup(_workplaceTexture == null);
            if (GUILayout.Button(ELT("export_workplace"), GUILayout.Width(_guiRect.width - 15f)))
            {
                GetWorkplaceExportor().Export(_workplaceTexture, GetDatas());
                Debug.Log("Export success!");
            }

            GUILayout.BeginHorizontal();

            // Set buttons to a fixed width
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

            // Checkerboard background
            GUI.DrawTextureWithTexCoords(
                new Rect(0, 0, _imageRect.width, _imageRect.height),
                _checkerBoardTexture,
                new Rect(0, 0, _imageRect.width / 16f, _imageRect.height / 16f)
            );

            // Workplace preview
            if (_workplaceTexture != null)
            {
                UpdatePreviewTexture((int)_imageRect.width, (int)_imageRect.height);

                GUI.DrawTexture(
                    new Rect(0, 0, _imageRect.width, _imageRect.height), // local group coords
                    _renderTexture,
                    ScaleMode.StretchToFill,
                    true
                );
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
                if (_renderTexture != null)
                    _renderTexture.Release();

                _renderTexture = new RenderTexture(previewWidth, previewHeight, 0)
                {
                    filterMode = FilterMode.Point
                };
            }

            RenderTexture.active = _renderTexture;

            // Clear with transparent or background color
            GL.Clear(true, true, Color.clear);

            GL.PushMatrix();
            GL.LoadPixelMatrix(0, _renderTexture.width, _renderTexture.height, 0);

            float invZoom = 1f / _zoom;

            // float halfPreviewW = _renderTexture.width * 0.5f;
            // float halfPreviewH = _renderTexture.height * 0.5f;
            // float halfSrcW = _workplaceTexture.width * 0.5f;
            // float halfSrcH = _workplaceTexture.height * 0.5f;

            // Compute UV in 0–1 coordinates
            // float uMin = (halfSrcW - halfPreviewW * invZoom + _panOffset.x * invZoom) / _workplaceTexture.width;
            // float vMin = (halfSrcH - halfPreviewH * invZoom + _panOffset.y * invZoom) / _workplaceTexture.height;
            // float uMax = (halfSrcW + halfPreviewW * invZoom + _panOffset.x * invZoom) / _workplaceTexture.width;
            // float vMax = (halfSrcH + halfPreviewH * invZoom + _panOffset.y * invZoom) / _workplaceTexture.height;

            _workplaceTexture.wrapMode = TextureWrapMode.Clamp;

            // Do NOT clamp
            // uMin = Mathf.Clamp01(uMin);
            // uMax = Mathf.Clamp01(uMax);
            // vMin = Mathf.Clamp01(vMin);
            // vMax = Mathf.Clamp01(vMax);

            float uvWidth = _renderTexture.width * invZoom / _workplaceTexture.width;
            float uvHeight = _renderTexture.height * invZoom / _workplaceTexture.height;

            float uCenter = 0.5f + _panOffset.x / _workplaceTexture.width;
            float vCenter = 0.5f + _panOffset.y / _workplaceTexture.height;

            Rect uvRect = new Rect(
                uCenter - uvWidth * 0.5f,
                vCenter - uvHeight * 0.5f,
                uvWidth,
                uvHeight
            );

            // Draw the texture into the RenderTexture
            Graphics.DrawTexture(
                new Rect(0, 0, _renderTexture.width, _renderTexture.height),
                _workplaceTexture,
                uvRect,
                0, 0, 0, 0
            );

            GL.PopMatrix();
            RenderTexture.active = null;
        }
    }
}