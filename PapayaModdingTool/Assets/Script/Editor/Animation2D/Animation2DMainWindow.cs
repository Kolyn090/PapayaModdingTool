using System.IO;
using PapayaModdingTool.Assets.Script.Editor.Universal;

namespace PapayaModdingTool.Assets.Script.Editor.Animation2D
{
    public class Animation2DMainWindow : MainWindow
    {
        public static void Open(string projectPath)
        {
            var window = GetWindow<Animation2DMainWindow>(Path.GetFileName(projectPath));
            window.Initialize(projectPath);
            window.Show();
        }

        protected override void OnGUI()
        {
            base.OnGUI();

        }
    }
}