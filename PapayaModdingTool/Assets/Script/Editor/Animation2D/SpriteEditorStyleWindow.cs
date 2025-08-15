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
        Vector2 previewCenter = new Vector2(previewRect.width * 0.5f, previewRect.height * 0.5f);

        float oldZoom = zoom;

        if (GUILayout.Button("Zoom In"))
        {
            zoom = Mathf.Min(zoom * 1.2f, ZOOM_MAX);
            panOffset += (previewCenter - panOffset) * (1 - oldZoom / zoom);
        }

        if (GUILayout.Button("Zoom Out"))
        {
            zoom = Mathf.Max(zoom / 1.2f, ZOOM_MIN);
            panOffset += (previewCenter - panOffset) * (1 - oldZoom / zoom);
        }

        GUILayout.EndHorizontal();

        // Limit pan based on zoomed texture size
        float panWidth = Mathf.Max(testTexture.width * zoom - previewRect.width, 0f) / 2f;
        float panHeight = Mathf.Max(testTexture.height * zoom - previewRect.height, 0f) / 2f;

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
