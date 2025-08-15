using System.IO;
using PapayaModdingTool.Assets.Script.Misc.Paths;
using UnityEditor;
using UnityEngine;

public class SpriteEditorPanelUI : EditorWindow
{
    private Texture2D testTexture;
    private Texture2D previewTexture;

    private Vector2 panOffset = Vector2.zero;  // in texture pixels
    private float zoom = 1f;
    private const float ZOOM_MIN = 1f;
    private const float ZOOM_MAX = 4f;

    [MenuItem("Window/SpriteEditor UI Controls")]
    public static void Open() => GetWindow<SpriteEditorPanelUI>("Sprite Editor UI");

    private void OnEnable()
    {
        byte[] imageData = File.ReadAllBytes(Path.Combine(
            string.Format(PredefinedPaths.PapayaTextureProjectPath, "Quan_D-2.11.0.6"),
            "unit_hero_quanhuying_psd_97f99a64c4a18168a8314aebe66b4d28_bundle",
            "unit_hero_quanhuying_-992531485953202068.png"));

        testTexture = new Texture2D(2, 2);
        testTexture.LoadImage(imageData);
        testTexture.Apply();
        testTexture.filterMode = FilterMode.Point;
    }

    private void OnGUI()
    {
        // --- UI Controls ---
        GUILayout.BeginHorizontal();

        Rect previewRect = new Rect(50, 150, 400, 300);

        // Zoom In
        if (GUILayout.Button("Zoom In"))
        {
            ZoomAtCenter(1.2f, previewRect);
        }

        // Zoom Out
        if (GUILayout.Button("Zoom Out"))
        {
            ZoomAtCenter(1f / 1.2f, previewRect);
        }

        GUILayout.EndHorizontal();

        // Compute pan bounds based on zoomed image size
        float panWidth = Mathf.Max(testTexture.width * zoom - previewRect.width, 0f) / 2f;
        float panHeight = Mathf.Max(testTexture.height * zoom - previewRect.height, 0f) / 2f;

        // Pan sliders
        panOffset.x = EditorGUILayout.Slider("Pan X", panOffset.x, -panWidth, panWidth);
        panOffset.y = EditorGUILayout.Slider("Pan Y", panOffset.y, -panHeight, panHeight);

        // --- Preview Panel ---
        GUI.Box(previewRect, "Preview");

        // Update and draw the preview texture
        GUI.BeginGroup(previewRect);
        UpdatePreviewTexture((int)previewRect.width, (int)previewRect.height);
        GUI.DrawTexture(new Rect(0, 0, previewRect.width, previewRect.height), previewTexture, ScaleMode.StretchToFill, true);
        GUI.EndGroup();
    }

    private void ZoomAtCenter(float zoomFactor, Rect previewRect)
    {
        float oldZoom = zoom;
        zoom = Mathf.Clamp(zoom * zoomFactor, ZOOM_MIN, ZOOM_MAX);

        // Compute half sizes
        Vector2 halfPreview = new Vector2(previewRect.width, previewRect.height) * 0.5f;
        Vector2 halfImageOld = new Vector2(testTexture.width, testTexture.height) * 0.5f * oldZoom;
        Vector2 halfImageNew = new Vector2(testTexture.width, testTexture.height) * 0.5f * zoom;

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
        float tX = (panOffset.x - oldPanMinX) / (oldPanMaxX - oldPanMinX);
        float tY = (panOffset.y - oldPanMinY) / (oldPanMaxY - oldPanMinY);

        panOffset.x = Mathf.Lerp(newPanMinX, newPanMaxX, tX);
        panOffset.y = Mathf.Lerp(newPanMinY, newPanMaxY, tY);

        // Clamp again to be safe
        panOffset.x = Mathf.Clamp(panOffset.x, newPanMinX, newPanMaxX);
        panOffset.y = Mathf.Clamp(panOffset.y, newPanMinY, newPanMaxY);
    }

    private void UpdatePreviewTexture(int previewWidth, int previewHeight)
    {
        if (previewTexture == null || previewTexture.width != previewWidth || previewTexture.height != previewHeight)
        {
            previewTexture = new Texture2D(previewWidth, previewHeight, TextureFormat.RGBA32, false);
            previewTexture.filterMode = FilterMode.Point;
        }

        Color[] srcPixels = testTexture.GetPixels();
        Color[] dstPixels = new Color[previewWidth * previewHeight];

        float invZoom = 1f / zoom;
        float halfPreviewW = previewWidth * 0.5f;
        float halfPreviewH = previewHeight * 0.5f;
        float halfSrcW = testTexture.width * 0.5f;
        float halfSrcH = testTexture.height * 0.5f;

        for (int y = 0; y < previewHeight; y++)
        {
            float srcYf = (y - halfPreviewH) * invZoom + halfSrcH - panOffset.y * invZoom;
            int srcYInt = Mathf.FloorToInt(srcYf);

            int dstRow = y * previewWidth;

            for (int x = 0; x < previewWidth; x++)
            {
                float srcXf = (x - halfPreviewW) * invZoom + halfSrcW - panOffset.x * invZoom;
                int srcXInt = Mathf.FloorToInt(srcXf);

                Color col = Color.clear;
                if (srcXInt >= 0 && srcXInt < testTexture.width && srcYInt >= 0 && srcYInt < testTexture.height)
                    col = srcPixels[srcYInt * testTexture.width + srcXInt];

                dstPixels[dstRow + x] = col;
            }
        }

        previewTexture.SetPixels(dstPixels);
        previewTexture.Apply();
    }
}
