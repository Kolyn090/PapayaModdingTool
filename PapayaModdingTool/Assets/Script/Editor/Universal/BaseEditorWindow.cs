using PapayaModdingTool.Assets.Script.Misc.AppCore;
using UnityEditor;

namespace PapayaModdingTool.Assets.Script.Editor.Universal
{
    public abstract class BaseEditorWindow : EditorWindow
    {
        protected static AppEnvironment _appEnvironment = new();
        private static EditorLocalization _localization = new(_appEnvironment.Wrapper.JsonSerializer);

        protected static string ELT(string tag) => _localization.ELT(tag);
    }
}
