using PapayaModdingTool.Assets.Script.DataStruct.EditorWindow;
using PapayaModdingTool.Assets.Script.Editor.Universal;
using UnityEditor;

namespace PapayaModdingTool.Assets.Script.Editor.Atlas2D
{
    public class Atlas2DStartWindow : BaseEditorWindow
    {
        [MenuItem("Tools/02 Atlas Maker")]
        public static void ShowWindow()
        {
            RecentProjectsWindow.ShowWindow(ELT("02_tool"), EditorWindowType.Atlas2D);
            Initialize();
        }
    }
}