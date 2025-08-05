using PapayaModdingTool.Assets.Script.Misc.AppCore;
using PapayaModdingTool.Assets.Script.Misc.Localization;
using UnityEditor;

namespace PapayaModdingTool.Assets.Script.Editor.Universal
{
    public abstract class BaseEditorWindow : EditorWindow
    {
        protected static AppEnvironment _appEnvironment = new();
        private static readonly EditorLocalization _localization = new(_appEnvironment.Wrapper.JsonSerializer);

        protected static string ELT(string tag) => _localization.ELT(tag);

        protected static void Initialize()
        {
            Language usingLanguage = _appEnvironment.AppSettingsManager.Reader.ReadLanguage();
            _localization.CurrentLanguage = LanguageUtil.LanguageToStr(usingLanguage);
        }
    }
}
