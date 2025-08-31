using System;
using System.Collections.Generic;
using PapayaModdingTool.Assets.Script.DataStruct.PreviewWorkplace;
using PapayaModdingTool.Assets.Script.DataStruct.TextureData;
using PapayaModdingTool.Assets.Script.Writer.Atlas2D;
using UnityEditor;
using UnityEngine;
using UEvent = UnityEngine.Event;

namespace PapayaModdingTool.Assets.Script.Editor.Atlas2DMainHelper
{
    public class PreviewTexturePanel
    {
        private const float ZOOM_MIN = 0.7f;
        private const float ZOOM_MAX = 4f;

        public Func<string, string> ELT;
        public Func<WorkplaceExportor> GetWorkplaceExportor;
        public Func<List<SpriteButtonData>> GetDatas;

        private Rect _guiRect;
        private Rect _imageRect;

        private bool _hasInit;

        private Texture2D _workplaceTexture;
        private RenderTexture _renderTexture;
        private Texture2D _checkerBoardTexture;
        private Vector2 _panOffset = Vector2.zero;
        private float _zoom = 1f;
        private List<SpriteButtonData> _workplace;
        private bool _needUpdateWorkplaceTexture = false;

#region Optimization
        private Vector2 _lastPanOffset;
        private float _lastZoom;
        private Vector2Int _lastRenderSize;

        private bool _isPanning = false;
        private Vector2 _lastMousePos;
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
                if (NeedsRenderUpdate())
                {
                    UpdatePreviewTexture((int)_imageRect.width, (int)_imageRect.height);
                    _lastRenderSize = new Vector2Int((int)_imageRect.width, (int)_imageRect.height);
                    _lastZoom = _zoom;
                    _lastPanOffset = _panOffset;
                }

                GUI.DrawTexture(
                    new Rect(0, 0, _imageRect.width, _imageRect.height), // local group coords
                    _renderTexture,
                    ScaleMode.StretchToFill,
                    true
                );
            }

            GUI.EndGroup();
            HandleMouseInput(_imageRect);
        }

        private bool NeedsRenderUpdate()
        {
            return _renderTexture == null
                || _lastRenderSize.x != (int)_imageRect.width
                || _lastRenderSize.y != (int)_imageRect.height
                || _lastZoom != _zoom
                || _lastPanOffset != _panOffset;
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
            if (_renderTexture == null ||
                _renderTexture.width != previewWidth ||
                _renderTexture.height != previewHeight)
            {
                if (_renderTexture != null)
                    _renderTexture.Release();

                _renderTexture = new RenderTexture(previewWidth, previewHeight, 0)
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

            float panelAspect = (float)previewWidth / previewHeight;
            float textureAspect = (float)_workplaceTexture.width / _workplaceTexture.height;
            Vector2 scale = Vector2.one;
            if (textureAspect > panelAspect) scale.y = panelAspect / textureAspect;
            else scale.x = textureAspect / panelAspect;
            blitMat.SetVector("_Scale", scale);

            Graphics.Blit(_workplaceTexture, _renderTexture, blitMat);

            // Restore previous RenderTexture
            RenderTexture.active = prev;
        }

        private void HandleMouseInput(Rect previewRect)
        {
            UEvent e = UEvent.current;

            // --- Zoom (scroll wheel) ---
            if (previewRect.Contains(e.mousePosition) && e.type == EventType.ScrollWheel)
            {
                float zoomFactor = (e.delta.y > 0) ? 1f / 1.1f : 1.1f;
                ZoomAtPosition(zoomFactor, e.mousePosition, previewRect);
                e.Use();
            }

            // --- Pan with Left Mouse ---
            if (e.type == EventType.MouseDown && e.button == 0 && previewRect.Contains(e.mousePosition))
            {
                _isPanning = true;
                _lastMousePos = e.mousePosition;
                e.Use(); // eat event so GUI buttons won’t get clicked
            }
            else if (e.type == EventType.MouseDrag && _isPanning && e.button == 0)
            {
                Vector2 deltaPanel = e.mousePosition - _lastMousePos;
                deltaPanel.y = -deltaPanel.y; // flip Y because IMGUI

                Vector2 panelToSrc = new Vector2(
                    _workplaceTexture.width  / previewRect.width,
                    _workplaceTexture.height / previewRect.height
                );

                _panOffset -= Vector2.Scale(deltaPanel, panelToSrc) / _zoom;
                _lastMousePos = e.mousePosition;

                e.Use();
            }
            else if (e.type == EventType.MouseUp && e.button == 0)
            {
                _isPanning = false;
            }
        }
        
        private void ZoomAtPosition(float zoomFactor, Vector2 mousePos, Rect previewRect)
        {
            Vector2 center = previewRect.center;

            // Mouse delta in panel space; flip Y because IMGUI Y+ is down
            Vector2 deltaPanel = mousePos - center;
            deltaPanel.y = -deltaPanel.y;

            // Convert panel pixels -> source (texture) pixels
            Vector2 panelToSrc = new Vector2(
                _workplaceTexture.width  / previewRect.width,
                _workplaceTexture.height / previewRect.height
            );

            float oldZoom = _zoom;
            float newZoom = Mathf.Clamp(_zoom * zoomFactor, ZOOM_MIN, ZOOM_MAX);
            float zoomChange = (newZoom / oldZoom) - 1f;

            _zoom = newZoom;

            // Adjust pan so the point under the cursor stays anchored
            _panOffset += Vector2.Scale(deltaPanel, panelToSrc) * zoomChange;
        }
    }
}