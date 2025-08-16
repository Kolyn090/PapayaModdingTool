using System.IO;
using PapayaModdingTool.Assets.Script.Editor.Universal;
using PapayaModdingTool.Assets.Script.Misc.Paths;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.Animation2D
{
    public class Animation2DMainWindow : MainWindow
    {
        private Texture2D _previewTexture;
        private PreviewTexturePanel _previewPanel;

        private void InitPreviewPanel()
        {
            // Testing
            byte[] imageData = File.ReadAllBytes(Path.Combine(
            string.Format(PredefinedPaths.PapayaTextureProjectPath, "Quan_D-2.11.0.6"),
            "unit_hero_quanhuying_psd_97f99a64c4a18168a8314aebe66b4d28_bundle",
            "unit_hero_quanhuying_-992531485953202068.png"));
            _previewTexture = new Texture2D(2, 2);
            _previewTexture.LoadImage(imageData);
            _previewTexture.Apply();
            _previewTexture.filterMode = FilterMode.Point;

            _previewPanel = new()
            {
                GetTexture = () => _previewTexture,
                ELT = var => ELT(var)
            };
            _previewPanel.Initialize(new(800, 100, 350, 600));
        }

        public static void Open(string projectPath)
        {
            var window = GetWindow<Animation2DMainWindow>(Path.GetFileName(projectPath));
            window.Initialize(projectPath);
            window.Show();
        }

        protected override void OnGUI()
        {
            base.OnGUI();
            if (_previewPanel == null)
            {
                InitPreviewPanel();
            }

            _previewPanel?.CreatePanel();
        }
    }
}