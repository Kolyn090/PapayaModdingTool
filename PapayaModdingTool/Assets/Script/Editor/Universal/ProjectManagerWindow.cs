using PapayaModdingTool.Assets.Script.Editor.Universal.ProjectManagerHelper;
using UnityEditor;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.Universal
{
    public class ProjectManagerWindow : BaseEditorWindow
    {
        private SwitchLanguageHelper _switchLanguageHelper;
        private DeleteProjectHelper _deleteProjectHelper;

        [MenuItem("Tools/__ Project Manager", false, 99)]
        public static void ShowWindow()
        {
            GetWindow<ProjectManagerWindow>(ELT("manage_project"));
            Initialize();
        }

        private void OnEnable()
        {
            _switchLanguageHelper = new()
            {
                ELT = var => ELT(var),
                CloseCurrWindow = Close,
                ShowCurrWindow = ShowWindow,
                ReadLanguage = _appEnvironment.AppSettingsManager.Reader.ReadLanguage,
                SetLanguage = _appEnvironment.AppSettingsManager.Writer.SetLanguage
            };
            _switchLanguageHelper.Initialize();

            _deleteProjectHelper = new()
            {
                ELT = var => ELT(var)
            };
        }

        private void OnGUI()
        {
            _switchLanguageHelper?.CreateSwitchLanguagePanel();

            GUILayout.Space(20);

            _deleteProjectHelper.CreateDeleteProjectPanel();
        }
    }
}