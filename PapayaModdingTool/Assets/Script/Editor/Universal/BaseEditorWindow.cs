using PapayaModdingTool.Assets.Script.Misc.AppCore;
using UnityEditor;

namespace PapayaModdingTool.Assets.Script.Editor.Universal
{
    public abstract class BaseEditorWindow : EditorWindow
    {
        protected static string ELT(string tag) => EditorLocalization.ELT(tag);
        protected static AppEnvironment _appEnvironment = new();
    }
}
