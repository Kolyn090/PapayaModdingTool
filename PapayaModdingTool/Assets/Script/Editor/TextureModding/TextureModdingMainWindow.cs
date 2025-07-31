using System.IO;
using PapayaModdingTool.Assets.Script.Editor.Universal;

namespace PapayaModdingTool.Assets.Script.Editor.TextureModding
{
    public class TextureModdingMainWindow : MainWindow
    {
        public static void Open(string projectName)
        {
            var window = GetWindow<TextureModdingMainWindow>(Path.GetFileName(projectName));
            window.Initialize(projectName);
            window.Show();
        }

        protected override void OnGUI()
        {
            base.OnGUI();

        }
    }
}