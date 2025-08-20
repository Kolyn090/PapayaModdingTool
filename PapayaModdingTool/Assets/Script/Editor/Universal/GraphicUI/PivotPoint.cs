using UnityEditor;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.Universal.GraphicUI
{
    public class PivotPoint
    {
        public static void MakePivot(Vector2 pivot, Rect drawRect)
        {
            MakePivot(pivot.x, pivot.y, drawRect);
        }

        public static void MakePivot(float pivotX, float pivotY, Rect drawRect)
        {
            Vector2 pivot = new(pivotX, pivotY);

            // Convert pivot to rect coordinates
            Vector2 pivotPos = new(
                drawRect.x + drawRect.width * pivot.x,
                drawRect.y + drawRect.height * (1 - pivot.y) // GUI Y flipped
            );

            Handles.BeginGUI();

            // Light blue donut (hollow circle) with anti-alias
            Handles.color = new Color(0.3f, 0.6f, 1f, 1f);
            float baseRadius = 6f;
            int segments = 64; // more segments = smoother circle

            Vector3[] points = new Vector3[segments + 1];
            for (int i = 0; i <= segments; i++)
            {
                float angle = i * Mathf.PI * 2 / segments;
                points[i] = pivotPos + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * baseRadius;
            }

            // Draw smooth circle
            Handles.DrawAAPolyLine(3f, points); // 3f = line thickness

            // White cross inside
            Handles.color = Color.white;
            float crossLength = 12f;
            float crossThickness = 3f;

            // Horizontal line
            Handles.DrawAAPolyLine(crossThickness,
                pivotPos + Vector2.left * crossLength,
                pivotPos + Vector2.right * crossLength);

            // Vertical line
            Handles.DrawAAPolyLine(crossThickness,
                pivotPos + Vector2.up * crossLength,
                pivotPos + Vector2.down * crossLength);

            Handles.EndGUI();
        }
    }
}