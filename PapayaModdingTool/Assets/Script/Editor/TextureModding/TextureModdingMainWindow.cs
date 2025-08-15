using System.Collections.Generic;
using System.IO;
using PapayaModdingTool.Assets.Script.Editor.TextureModding.MainWindowHelper;
using PapayaModdingTool.Assets.Script.Editor.Universal;
using PapayaModdingTool.Assets.Script.Editor.Writer.TextureModding;
using PapayaModdingTool.Assets.Script.Reader.ProjectUtil;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.TextureModding
{
    public class TextureModdingMainWindow : MainWindow
    {
        private readonly ProjectLoader _projectLoader = new();
        private readonly TextureAssetsLoader _textureAssetsLoader = new(_appEnvironment);
        private static List<string> _loadedPaths = null;
        private bool _loadedPathsChanged = true;

        private LoadFilesHelper _loadFilesHelper;
        private RemoveLoadedHelper _removeLoadedHelper;
        private InstallLoadedHelper _installLoadedHelper;

        private void OnEnable()
        {
            _loadFilesHelper = new()
            {
                GetLoadedPaths = () => _loadedPaths,
                SetLoadedPaths = var => _loadedPaths = var,
                GetLoadedPathsChanged = () => _loadedPathsChanged,
                SetLoadedPathsChanged = var => _loadedPathsChanged = var,
                GetProjectName = () => ProjectName,
                LoadPathsFromDB = () => _projectLoader.FindLoadedPaths(ProjectName, _appEnvironment.Wrapper.JsonSerializer),
                GetTextureAssetsLoader = () => _textureAssetsLoader,
                GetAppEnvironment = () => _appEnvironment,
                ELT = var => ELT(var)
            };

            _removeLoadedHelper = new()
            {
                ELT = var => ELT(var),
                GetLoadedPaths = () => _loadedPaths,
                SetLoadedPathsChangedToTrue = () => _loadedPathsChanged = true,
                GetJsonSerializer = () => _appEnvironment.Wrapper.JsonSerializer,
                GetProjectName = () => ProjectName
            };

            _installLoadedHelper = new()
            {
                ELT = var => ELT(var),
                GetAppEnvironment = () => _appEnvironment,
                GetLoadedPaths = () => _loadedPaths,
                GetProjectName = () => ProjectName,
                GetTextureAssetsLoader = () => _textureAssetsLoader
            };
            Initialize();
        }

        public static void Open(string projectPath)
        {
            var window = GetWindow<TextureModdingMainWindow>(Path.GetFileName(projectPath));
            window.Initialize(projectPath);
            window.Show();
        }

        protected override void OnGUI()
        {
            base.OnGUI();

            _loadFilesHelper.CreateLoadedPathsPanel();

            GUILayout.Space(20);

            _removeLoadedHelper.CreateRemoveLoadedPanel();

            GUILayout.Space(20);

            _installLoadedHelper.CreateInstallLoadedPanel();
        }
    }
}