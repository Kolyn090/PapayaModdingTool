using UnityEditor;
using UnityEngine;
using UEvent = UnityEngine.Event;

namespace PapayaModdingTool.Assets.Script.Editor.Universal.GraphicUI
{
    public class VerticalScrollbar
    {
        private bool _draggingThumb = false;
        private float _dragOffsetY = 0f;

        /// <summary>
        /// Draws a vertical scrollbar inside a given viewport and updates scrollPos.y.
        /// </summary>
        /// <param name="viewport">Rect of the visible scroll area</param>
        /// <param name="totalContentHeight">Total height of the scrollable content</param>
        /// <param name="scrollPos">Reference to the scroll position vector</param>
        /// <param name="scrollBarY">Y offset of the scrollbar within the viewport</param>
        /// <param name="scrollbarWidth">Width of the scrollbar in pixels</param>
        void Draw(Rect viewport,
                                        float totalContentHeight,
                                        ref Vector2 scrollPos,
                                        float scrollBarY,
                                        float scrollbarWidth = 12f)
        {
            float viewportHeight = viewport.height;

            // Thumb height (min 20 px)
            float thumbHeight = Mathf.Max(20f, viewportHeight / Mathf.Max(1f, totalContentHeight) * viewportHeight);

            // Thumb Y position relative to the viewport
            float thumbY = scrollBarY + scrollPos.y / Mathf.Max(1f, totalContentHeight - viewportHeight) * (viewportHeight - thumbHeight);

            // Scrollbar background
            Rect scrollbarRect = new Rect(viewport.width - scrollbarWidth, scrollBarY, scrollbarWidth, viewportHeight);
            EditorGUI.DrawRect(scrollbarRect, new Color(0.1f, 0.1f, 0.1f));

            // Scrollbar thumb
            Rect thumbRect = new Rect(scrollbarRect.x, thumbY, scrollbarWidth, thumbHeight);
            EditorGUI.DrawRect(thumbRect, new Color(0.6f, 0.6f, 0.6f));

            // Handle mouse wheel
            if (UEvent.current.type == EventType.ScrollWheel && viewport.Contains(UEvent.current.mousePosition))
            {
                scrollPos.y += UEvent.current.delta.y * 20f; // scroll speed
                scrollPos.y = Mathf.Clamp(scrollPos.y, 0, Mathf.Max(0, totalContentHeight - viewportHeight));
                UEvent.current.Use();
            }

            // Handle thumb dragging
            if (UEvent.current.type == EventType.MouseDown && thumbRect.Contains(UEvent.current.mousePosition))
            {
                _draggingThumb = true;
                _dragOffsetY = UEvent.current.mousePosition.y - thumbRect.y;
                UEvent.current.Use();
            }

            if (_draggingThumb && UEvent.current.type == EventType.MouseDrag)
            {
                float newThumbY = UEvent.current.mousePosition.y - _dragOffsetY;
                newThumbY = Mathf.Clamp(newThumbY, scrollBarY, scrollBarY + viewportHeight - thumbHeight);
                scrollPos.y = (newThumbY - scrollBarY) / (viewportHeight - thumbHeight) * (totalContentHeight - viewportHeight);
                UEvent.current.Use();
            }

            if (_draggingThumb && UEvent.current.type == EventType.MouseUp)
            {
                _draggingThumb = false;
                UEvent.current.Use();
            }
        }
    }
}