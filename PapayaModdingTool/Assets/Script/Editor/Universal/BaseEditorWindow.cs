using UnityEditor;

namespace PapayaModdingTool.Assets.Script.Editor.Universal
{
    public abstract class BaseEditorWindow : EditorWindow
    {
        protected static string ELT(string tag) => EditorLocalization.ELT(tag);
    }
}
