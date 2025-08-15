using System.IO;
using PapayaModdingTool.Assets.Script.Editor.Universal;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.Animation2D
{
    public class Animation2DMainWindow : MainWindow
    {
        private PanZoomPanel _previewPanel;

        private void OnEnable()
        {
            _previewPanel = new();
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
            _previewPanel.CreatePanZoomPanel();
        }
    }
}