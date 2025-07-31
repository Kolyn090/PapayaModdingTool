using UnityEditor;

namespace PapayaModdingTool.Assets.Script.Editor.TextureModding
{
    public class StartWindow : EditorWindow
    {
        [MenuItem("Tools/01 Texture Modding")]
        public static void ShowWindow()
        {
            RecentProjectsWindow.ShowWindow("01 Texture Modding");
        }
    }
}