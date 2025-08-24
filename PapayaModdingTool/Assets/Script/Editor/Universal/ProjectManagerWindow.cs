using PapayaModdingTool.Assets.Script.Editor.Universal.ProjectManagerHelper;
using PapayaModdingTool.Assets.Script.Editor.Writer.Universal;
using UnityEditor;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.Universal
{
    public class ProjectManagerWindow : BaseEditorWindow
    {
        private readonly ProjectRemover _projectRemover = new(_appEnvironment.Wrapper.JsonSerializer);

        private SwitchLanguageHelper _switchLanguageHelper;
        private DeleteProjectHelper _deleteProjectHelper;

        private bool _hasInit = false;

        [MenuItem("Tools/__ Project Manager", false, 99)]
        public static void ShowWindow()
        {
            Initialize();
            GetWindow<ProjectManagerWindow>(ELT("manage_project"));
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
                ELT = var => ELT(var),
                GetProjectRemover = () => _projectRemover
            };

            _hasInit = true;
        }

        private void OnGUI()
        {
            if (!_hasInit)
                OnEnable();

            _switchLanguageHelper?.CreateSwitchLanguagePanel();

            GUILayout.Space(20);

            _deleteProjectHelper.CreateDeleteProjectPanel();
        }
    }
}