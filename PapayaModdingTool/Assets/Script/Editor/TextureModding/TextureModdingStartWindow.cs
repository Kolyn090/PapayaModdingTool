using PapayaModdingTool.Assets.Script.DataStruct.EditorWindow;
using PapayaModdingTool.Assets.Script.Editor.Universal;
using UnityEditor;

namespace PapayaModdingTool.Assets.Script.Editor.TextureModding
{
    public class TextureModdingStartWindow : BaseEditorWindow
    {
        [MenuItem("Tools/01 Texture Modding")]
        public static void ShowWindow()
        {
            RecentProjectsWindow.ShowWindow(ELT("01_tool"), EditorWindowType.TextureModding);
        }
    }
}
