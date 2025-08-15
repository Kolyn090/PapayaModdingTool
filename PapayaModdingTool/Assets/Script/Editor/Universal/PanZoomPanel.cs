using UnityEngine;
using UEvent = UnityEngine.Event;

namespace PapayaModdingTool.Assets.Script.Editor.Universal
{
    public class PanZoomPanel
    {
        private const float ZOOM_MIN = 0.5f;
        private const float ZOOM_MAX = 4.0f;

        private Vector2 _panOffset;
        private readonly float _panFactor = 1.5f;
        private float _zoom = 1.0f;

        public void CreatePanZoomPanel()
        {
            Rect maskRect = new Rect(50, 50, 400, 300);
            GUI.Box(maskRect, "Preview Result");

            // Begin clipping to maskRect
            GUI.BeginGroup(maskRect);

            // Convert mouse to local coordinates
            UEvent e = UEvent.current;
            Vector2 localMouse = e.mousePosition;

            // Handle zoom
            if (maskRect.Contains(e.mousePosition) && e.type == EventType.ScrollWheel)
            {
                Vector2 zoomCenter = localMouse;
                float oldZoom = _zoom;
                _zoom = Mathf.Clamp(_zoom * (1 - e.delta.y / 150f), 0.2f, 2f);
                _panOffset -= (zoomCenter - _panOffset) - (oldZoom / _zoom) * (zoomCenter - _panOffset);
                e.Use();
            }

            // Handle pan (middle mouse drag)
            if (maskRect.Contains(e.mousePosition) && e.type == EventType.MouseDrag && e.button == 2)
            {
                _panOffset += e.delta;
                e.Use();
            }

            // Save matrix
            Matrix4x4 oldMatrix = GUI.matrix;

            // Apply pan & zoom
            GUIUtility.ScaleAroundPivot(Vector2.one * _zoom, _panOffset);

            // Draw content relative to group
            Rect contentRect = new Rect(0, 0, 1000, 1000);
            GUI.Box(contentRect, "Zoomable Content");
            GUI.Label(new Rect(50, 50, 200, 20), $"Zoom: {_zoom:F2}");
            GUI.Label(new Rect(50, 80, 200, 20), $"Pan: {_panOffset}");

            // Restore matrix
            GUI.matrix = oldMatrix;

            // End clipping group
            GUI.EndGroup();
        }
    }
}