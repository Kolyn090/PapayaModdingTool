using System.Collections.Generic;
using System.IO;
using PapayaModdingTool.Assets.Script.DataStruct.FileRead;
using PapayaModdingTool.Assets.Script.Editor.Universal;
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
        private Vector2 _scrollPos;
        private string _removeLoadedPath;

        public static void Open(string projectPath)
        {
            var window = GetWindow<TextureModdingMainWindow>(Path.GetFileName(projectPath));
            window.Initialize(projectPath);
            window.Show();
        }

        protected override void OnGUI()
        {
            base.OnGUI();

            GUIStyle rightAlignedStyle = new(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleRight
            };

            List<string> loadedPaths = _projectLoader.FindLoadedPaths(Path.GetFileName(_projectPath), _appEnvironment.Wrapper.JsonSerializer);

            GUILayout.Space(20);

            // Scroll view for loaded paths
            GUILayout.Label(ELT("loaded_files"), EditorStyles.boldLabel);
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Height(50));
            foreach (var item in loadedPaths)
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

        }

        private void LoadNewFile()
        {
            LoadFileInfo? loadInfo = _projectWriter.LoadNewFile(Path.GetFileName(_projectPath),
                                        _appEnvironment.Wrapper.FileBrowser,
                                        _appEnvironment.Wrapper.JsonSerializer);
            if (loadInfo == null)
                return;
        }

        private void AttemptToLoadTexture()
        {
            
        }
    }
}