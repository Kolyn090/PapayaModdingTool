using System;
using UnityEngine;
using UEvent = UnityEngine.Event;

namespace PapayaModdingTool.Assets.Script.Editor.Atlas2D.Atlas2DHelper
{
    public class ZoomPanController
    {
        private const float ZOOM_MIN = 0.7f;
        private const float ZOOM_MAX = 4f;

        private Vector2 _panOffset = Vector2.zero;
        public Vector2 PanOffset => _panOffset;
        private float _zoom = 1f;
        public float Zoom => _zoom;

        private Vector2 _lastPanOffset;
        private float _lastZoom;
        private Vector2Int _lastRenderSize;

        private bool _isPanning = false;
        private Vector2 _lastMousePos;

        private Rect _imageRect;
        private readonly Func<Texture2D> _GetWorkplaceTexture;
        private float GetWorkplaceTextureWidth => _GetWorkplaceTexture != null ? _GetWorkplaceTexture().width : 0;
        private float GetWorkplaceTextureHeight => _GetWorkplaceTexture != null ? _GetWorkplaceTexture().height : 0;

        public bool HasChanged =>
            _lastRenderSize.x != (int)_imageRect.width ||
            _lastRenderSize.y != (int)_imageRect.height ||
            _lastZoom != _zoom ||
            _lastPanOffset != _panOffset;

        public void SetPanOffsetX(float x)
        {
            _panOffset.x = x;
        }

        public void SetPanOffsetY(float y)
        {
            _panOffset.y = y;
        }

        public void UpdateLast()
        {
            _lastRenderSize = new Vector2Int((int)_imageRect.width, (int)_imageRect.height);
            _lastZoom = _zoom;
            _lastPanOffset = _panOffset;
        }

        public ZoomPanController(Rect imageRect, Func<Texture2D> GetWorkplaceTexture)
        {
            _imageRect = imageRect;
            _GetWorkplaceTexture = GetWorkplaceTexture;
        }

        public void ZoomAtCenter(float zoomFactor, Rect previewRect)
        {
            float oldZoom = _zoom;
            _zoom = Mathf.Clamp(_zoom * zoomFactor, ZOOM_MIN, ZOOM_MAX);

            // Compute half sizes
            Vector2 halfPreview = new Vector2(previewRect.width, previewRect.height) * 0.5f;
            Vector2 halfImageOld = 0.5f * oldZoom * new Vector2(GetWorkplaceTextureWidth, GetWorkplaceTextureHeight);
            Vector2 halfImageNew = _zoom * 0.5f * new Vector2(GetWorkplaceTextureWidth, GetWorkplaceTextureHeight);

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

        public void HandleMouseInput(Rect previewRect)
        {
            UEvent e = UEvent.current;

            // --- Zoom (scroll wheel) ---
            if (previewRect.Contains(e.mousePosition) && e.type == EventType.ScrollWheel)
            {
                float zoomFactor = (e.delta.y > 0) ? 1f / 1.1f : 1.1f;
                ZoomAtPosition(zoomFactor, e.mousePosition, previewRect);
                e.Use();
            }

            // --- Pan with Left Mouse or Mid Mouse ---
            if (e.type == EventType.MouseDown && (e.button == 0 || e.button == 2) && previewRect.Contains(e.mousePosition))
            {
                _isPanning = true;
                _lastMousePos = e.mousePosition;
                e.Use(); // eat event so GUI buttons won’t get clicked
            }
            else if (e.type == EventType.MouseDrag && _isPanning && (e.button == 0 || e.button == 2))
            {
                Vector2 deltaPanel = e.mousePosition - _lastMousePos;
                deltaPanel.y = -deltaPanel.y; // flip Y because IMGUI

                Vector2 panelToSrc = new(
                    GetWorkplaceTextureWidth / previewRect.width,
                    GetWorkplaceTextureHeight / previewRect.height
                );

                _panOffset -= Vector2.Scale(deltaPanel, panelToSrc) / _zoom;
                _lastMousePos = e.mousePosition;

                e.Use();
            }
            else if (e.type == EventType.MouseUp && (e.button == 0 || e.button == 2))
            {
                _isPanning = false;
            }
        }

        private void ZoomAtPosition(float zoomFactor, Vector2 mousePos, Rect previewRect)
        {
            float oldZoom = _zoom;
            _zoom = Mathf.Clamp(_zoom * zoomFactor, ZOOM_MIN, ZOOM_MAX);
            float zoomRatio = _zoom / oldZoom;

            // Mouse position relative to panel top-left
            Vector2 localMouse = mousePos - previewRect.position;

            // Compute offset from panel center in GUI coordinates
            Vector2 offsetFromCenter = localMouse - previewRect.size * 0.5f;

            // Adjust pan so the zoom pivots around the mouse
            _panOffset += offsetFromCenter * (1f - zoomRatio);
        }
    }
}