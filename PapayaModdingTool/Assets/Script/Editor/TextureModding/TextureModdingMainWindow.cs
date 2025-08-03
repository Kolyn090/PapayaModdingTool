using System.Collections.Generic;
using System.IO;
using PapayaModdingTool.Assets.Script.DataStruct.FileRead;
using PapayaModdingTool.Assets.Script.Editor.Universal;
using PapayaModdingTool.Assets.Script.Editor.Writer.TextureModding;
using PapayaModdingTool.Assets.Script.Editor.Writer.Universal;
using PapayaModdingTool.Assets.Script.Misc.Paths;
using PapayaModdingTool.Assets.Script.Reader.ProjectUtil;
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
        private static List<string> _loadedPaths = null;
        private bool _loadedPathsChanged = true;

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
            _textureAssetsLoader.LoadTextureAssets(loadInfo, ProjectName, textureSaveDir);
        }

        private void RemoveTypedFile()
        {
            _projectRemover.RemoveFileFromProject(_removeLoadedPath, ProjectName, LoadType.Texture);
        }
    }
}