using System.IO;
using PapayaModdingTool.Assets.Script.Misc.Paths;
using UnityEditor;
using UnityEngine;

public class SpriteEditorPanelUI : EditorWindow
{
    private Texture2D testTexture;
    private Vector2 panOffset = Vector2.zero;
    private float zoom = 1f;
    private const float ZOOM_MIN = 1f;
    private const float ZOOM_MAX = 4f;
    Texture2D previewTexture;

    [MenuItem("Window/SpriteEditor UI Controls")]
    public static void Open() => GetWindow<SpriteEditorPanelUI>("Sprite Editor UI");

    private void OnEnable()
    {
        byte[] imageData = File.ReadAllBytes(Path.Combine(string.Format(PredefinedPaths.PapayaTextureProjectPath, "Quan_D-2.11.0.6"), "unit_hero_quanhuying_psd_97f99a64c4a18168a8314aebe66b4d28_bundle", "unit_hero_quanhuying_-992531485953202068.png"));
        // Just a test texture
        testTexture = new(2, 2);
        testTexture.LoadImage(imageData); // auto-resizes texture
        testTexture.Apply();
        testTexture.filterMode = FilterMode.Point;
    }

    private void OnGUI()
    {
        // --- UI Controls ---
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Zoom In")) zoom = Mathf.Min(zoom * 1.2f, ZOOM_MAX);
        if (GUILayout.Button("Zoom Out")) zoom = Mathf.Max(zoom / 1.2f, ZOOM_MIN);
        GUILayout.EndHorizontal();

        float panWidth = testTexture.width * zoom / 2;
        float panHeight = testTexture.height * zoom / 2;
        // Pan sliders
        panOffset.x = EditorGUILayout.Slider("Pan X", panOffset.x, -panWidth, panWidth);
        panOffset.y = EditorGUILayout.Slider("Pan Y", panOffset.y, -panHeight, panHeight);

        // --- Preview Panel ---
        Rect previewRect = new Rect(50, 150, 400, 300);
        GUI.Box(previewRect, "Preview");

        GUI.BeginGroup(previewRect);
        UpdatePreviewTexture((int)previewRect.width, (int)previewRect.height);
        GUI.DrawTexture(new Rect(0, 0, previewRect.width, previewRect.height), previewTexture, ScaleMode.StretchToFill, true);

        GUI.EndGroup();
    }

    void UpdatePreviewTexture(int previewWidth, int previewHeight)
    {
        if (previewTexture == null ||
            previewTexture.width != previewWidth ||
            previewTexture.height != previewHeight)
        {
            previewTexture = new Texture2D(previewWidth, previewHeight, TextureFormat.RGBA32, false);
            previewTexture.filterMode = FilterMode.Point;
        }

        // Determine what portion of testTexture to sample
        for (int y = 0; y < previewHeight; y++)
        {
            for (int x = 0; x < previewWidth; x++)
            {
                // Convert preview coords into source texture coords
                float srcX = (x - previewWidth  / 2f) / zoom + testTexture.width  / 2f - panOffset.x / zoom;
                float srcY = (y - previewHeight / 2f) / zoom + testTexture.height / 2f - panOffset.y / zoom;
                Color col = Color.clear;
                if (srcX >= 0 && srcX < testTexture.width && srcY >= 0 && srcY < testTexture.height)
                {
                    col = testTexture.GetPixel((int)srcX, (int)srcY);
                }
                previewTexture.SetPixel(x, y, col);
            }
        }

        previewTexture.Apply();
    }
}
