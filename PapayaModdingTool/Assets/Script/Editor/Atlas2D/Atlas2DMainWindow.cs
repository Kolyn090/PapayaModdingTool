using System.IO;
using PapayaModdingTool.Assets.Script.Editor.Universal;

using UEvent = UnityEngine.Event;

namespace PapayaModdingTool.Assets.Script.Editor.Atlas2D
{
    public class Atlas2DMainWindow : MainWindow
    {
        private readonly Atlas2DMain _atlas2DMain = new();

        public static void Open(string projectPath)
        {
            var window = GetWindow<Atlas2DMainWindow>(Path.GetFileName(projectPath));
            window.Initialize(projectPath);
            window.Show();
        }

        protected override void OnGUI()
        {
            base.OnGUI();
            if (!_atlas2DMain.HasInit)
            {
                _atlas2DMain.Initialize(ELT, _appEnvironment, ProjectName);
            }

            UEvent e = UEvent.current;
            _atlas2DMain.SpriteEditShortcutManager?.HandleEvent(e);
            _atlas2DMain.SpritesPanelShortcutManager?.HandleEvent(e);
            _atlas2DMain.PreviewTexturePanel?.CreatePanel();
            _atlas2DMain.SpritesPanel?.CreatePanel();
            _atlas2DMain.SpriteEditPanel?.CreatePanel();
            _atlas2DMain.TexturesPanel?.CreatePanel();
        }
    }
}