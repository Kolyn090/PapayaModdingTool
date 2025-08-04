using System.Collections.Generic;
using System.IO;
using PapayaModdingTool.Assets.Script.DataStruct.FileRead;
using PapayaModdingTool.Assets.Script.Editor.Universal;
using PapayaModdingTool.Assets.Script.Editor.Writer.ProjectUtil;
using PapayaModdingTool.Assets.Script.Editor.Writer.TextureModding;
using PapayaModdingTool.Assets.Script.Editor.Writer.Universal;
using PapayaModdingTool.Assets.Script.Misc.Paths;
using PapayaModdingTool.Assets.Script.Reader.ProjectUtil;
using PapayaModdingTool.Assets.Script.Writer.AddressableTools;
using PapayaModdingTool.Assets.Script.Writer.ProjectUtil;
using UnityEditor;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.TextureModding
{
    public class TextureModdingMainWindow : MainWindow
    {
        private readonly ProjectLoader _projectLoader = new();
        private readonly ProjectWriter _projectWriter = new();
        private static ProjectRemover _projectRemover;
        private readonly TextureAssetsLoader _textureAssetsLoader = new(_appEnvironment);
        private Vector2 _scrollPos;
        private string _removeLoadedPath;
        private string _installLoadedPath;
        private string _catalogPath;
        private int _ppu = 100;
        private static List<string> _loadedPaths = null;
        private bool _loadedPathsChanged = true;
        private BuildTarget _buildPlatform = BuildTarget.StandaloneWindows64;

        public static void Open(string projectPath)
        {
            var window = GetWindow<TextureModdingMainWindow>(Path.GetFileName(projectPath));
            window.Initialize(projectPath);
            window.Show();

            _projectRemover ??= new(_appEnvironment.Wrapper.JsonSerializer);
        }

        protected override void OnGUI()
        {
            base.OnGUI();

            GUIStyle rightAlignedStyle = new(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleRight
            };

            _ppu = EditorGUILayout.IntField(ELT("ppu"), _ppu);

            if (_loadedPaths == null || _loadedPathsChanged)
            {
                _loadedPaths = _projectLoader.FindLoadedPaths(ProjectName, _appEnvironment.Wrapper.JsonSerializer);
                _loadedPathsChanged = false;
            }

            GUILayout.Space(20);

            // Scroll view for loaded paths
            GUILayout.Label(ELT("loaded_files"), EditorStyles.boldLabel);
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Height(50));
            foreach (var item in _loadedPaths)
            {
                EditorGUILayout.SelectableLabel(item, rightAlignedStyle, GUILayout.Height(15));
            }
            EditorGUILayout.EndScrollView();
            GUILayout.Space(5);
            if (GUILayout.Button(ELT("load_new")))
            {
                LoadNewFile();
            }

            GUILayout.Space(20);

            // * Remove
            GUILayout.Label(ELT("remove_loaded"), EditorStyles.boldLabel);
            _removeLoadedPath = EditorGUILayout.TextField("", _removeLoadedPath);
            bool isRemoveValid = _loadedPaths.Contains(_removeLoadedPath);
            if (!isRemoveValid)
            {
                EditorGUILayout.HelpBox(ELT("enter_exact_path_to_delete"), MessageType.Info);
            }
            EditorGUI.BeginDisabledGroup(!isRemoveValid);
            if (GUILayout.Button(ELT("remove_file")))
            {
                RemoveTypedFile();
                _loadedPathsChanged = true;
            }
            EditorGUI.EndDisabledGroup();

            GUILayout.Space(20);

            // * Install
            _buildPlatform = (BuildTarget)EditorGUILayout.EnumPopup(ELT("build_target"), _buildPlatform);
            GUILayout.Space(5);
            // Patch catalog.json
            EditorGUILayout.BeginHorizontal();
            _catalogPath = EditorGUILayout.TextField(ELT("catalog_patch"), _catalogPath);
            if (GUILayout.Button(ELT("browse"), GUILayout.Width(60)))
            {
                string[] results = _appEnvironment.Wrapper.FileBrowser.OpenFilePanel("Search catalog.json", "", new[] { "json" }, false);
                if (results.Length > 0)
                {
                    _catalogPath = results[0];
                }
                else
                {
                    Debug.LogWarning("Failed to find catalog.json.");
                }
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5);
            GUILayout.Label(ELT("install_modified"), EditorStyles.boldLabel);
            _installLoadedPath = EditorGUILayout.TextField("", _installLoadedPath);
            bool isInstallValid = _loadedPaths.Contains(_installLoadedPath);
            if (!isInstallValid)
            {
                EditorGUILayout.HelpBox(ELT("enter_exact_path_to_install"), MessageType.Info);
            }
            EditorGUI.BeginDisabledGroup(!isInstallValid);
            if (GUILayout.Button(ELT("install_modified_file")))
            {
                List<(string, string)> tags = AssignTag();
                BuildAssetBundle();
                ExportTextures(tags);
                ExportOwningDumps(tags);
                if (!string.IsNullOrWhiteSpace(_catalogPath) && Directory.Exists(_catalogPath))
                {
                    AddrTool.PatchCrc(_catalogPath);
                }
            }
            EditorGUI.EndDisabledGroup();
        }

        private void LoadNewFile()
        {
            LoadFileInfo? loadInfo = _projectWriter.LoadNewFile(ProjectName,
                                        _appEnvironment.Wrapper.FileBrowser,
                                        _appEnvironment.Wrapper.JsonSerializer,
                                        LoadType.Texture);
            if (loadInfo == null)
                return;

            AttemptToLoadTexture((LoadFileInfo)loadInfo);
            _loadedPathsChanged = true;
        }

        private void AttemptToLoadTexture(LoadFileInfo loadInfo)
        {
            string textureDir = PredefinedPaths.PapayaTextureDir;
            string textureSaveDir = Path.Combine(textureDir, ProjectName, loadInfo.folder);
            if (!Directory.Exists(textureSaveDir))
                Directory.CreateDirectory(textureSaveDir);
            _textureAssetsLoader.LoadTextureAssets(loadInfo, ProjectName, textureSaveDir, _ppu);
        }

        private void RemoveTypedFile()
        {
            _projectRemover.RemoveFileFromProject(_removeLoadedPath, ProjectName, LoadType.Texture);
        }

        // Assign asset bundle tag for Texture2D / Atlas (in the future)
        // Return the processed tags (which are the names of the built AssetBundles)
        private List<(string, string)> AssignTag()
        {
            List<(string, string)> result = new();

            // Find all existing Texture assets under install loading path
            string unityTexturePath = Path.Combine(PredefinedPaths.PapayaUnityDir,
                                                    "Texture",
                                                    ProjectName);

            string[] texturePaths = Directory.GetDirectories(unityTexturePath, "*", SearchOption.TopDirectoryOnly);
            foreach (string path in texturePaths)
            {
                string assetBundleTag = string.Join('_', new string[] {
                    ProjectName,
                    "texture",
                    Path.GetFileName(path)
                });
                result.Add((assetBundleTag, Path.GetFileName(path)));
                string[] files = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly);
                foreach (string file in files)
                {
                    // Only assign tag if it has meta file
                    string metaForFile = file + ".meta";
                    if (File.Exists(metaForFile))
                    {
                        AssetBundleTagEditor.SetAssetBundleTag(file, assetBundleTag);
                    }
                }
            }
            return result;
        }

        private void BuildAssetBundle()
        {
            string assetBundlesPath = Path.Combine(PredefinedPaths.PapayaUnityDir,
                                                    "AssetBundles");
            string savingPath = assetBundlesPath;
            AssetBundleBuilder.BuildAllAssetBundles(savingPath, _buildPlatform);
        }

        // Export the Texture from the AssetBundle created by program to External
        private void ExportTextures(List<(string, string)> bundleFileNames)
        {
            // Save to External/Project/Texture/Exported
            // Load from Papaya_Unity/AssetBundles
            string assetBundlesPath = Path.Combine(PredefinedPaths.PapayaUnityDir,
                                                    "AssetBundles");
            foreach ((string, string) bundleFileName in bundleFileNames)
            {
                (string bundleName, string fileName) = bundleFileName;
                string bundlePath = Path.Combine(assetBundlesPath, bundleName);
                string textureSavePath = Path.Combine(PredefinedPaths.ProjectsPath,
                                                        ProjectName,
                                                        fileName,
                                                        "Texture/Exported");
                _textureAssetsLoader.LoadTextureOnly(bundlePath, textureSavePath);
            }
        }

        private void ExportOwningDumps(List<(string, string)> bundleFileNames)
        {
            string assetBundlesPath = Path.Combine(PredefinedPaths.PapayaUnityDir,
                                                    "AssetBundles");
            foreach ((string, string) bundleFileName in bundleFileNames)
            {
                (string bundleName, string fileName) = bundleFileName;
                string bundlePath = Path.Combine(assetBundlesPath, bundleName);
                _textureAssetsLoader.ExportSpriteDumpsOnly(bundlePath, fileName, ProjectName);
            }
        }
    }
}