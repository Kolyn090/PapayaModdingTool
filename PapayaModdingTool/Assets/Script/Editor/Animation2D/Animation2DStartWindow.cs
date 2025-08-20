using PapayaModdingTool.Assets.Script.DataStruct.EditorWindow;
using PapayaModdingTool.Assets.Script.Editor.Universal;
using UnityEditor;

namespace PapayaModdingTool.Assets.Script.Editor.Animation2D
{
    public class Animation2DStartWindow : BaseEditorWindow
    {
        [MenuItem("Tools/02 Atlas Maker")]
        public static void ShowWindow()
        {
            RecentProjectsWindow.ShowWindow(ELT("02_tool"), EditorWindowType.Animation2D);
            Initialize();
        }
    }
}