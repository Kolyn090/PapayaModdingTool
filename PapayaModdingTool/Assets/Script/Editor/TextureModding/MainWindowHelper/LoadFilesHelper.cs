using System;
using System.Collections.Generic;
using System.IO;
using PapayaModdingTool.Assets.Script.DataStruct.FileRead;
using PapayaModdingTool.Assets.Script.Editor.Writer.TextureModding;
using PapayaModdingTool.Assets.Script.Misc.AppCore;
using PapayaModdingTool.Assets.Script.Misc.Paths;
using PapayaModdingTool.Assets.Script.Writer.ProjectUtil;
using UnityEditor;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.TextureModding.MainWIndowHelper
{
    public class LoadFilesHelper
    {
        public Func<List<string>> GetLoadedPaths;
        public Action<List<string>> SetLoadedPaths;
        public Func<bool> GetLoadedPathsChanged;
        public Action<bool> SetLoadedPathsChanged;
        public Func<string> GetProjectName;
        public Func<List<string>> LoadPathsFromDB;
        public Func<TextureAssetsLoader> GetTextureAssetsLoader;
        public Func<AppEnvironment> GetAppEnvironment;
        public Func<string, string> ELT;
        private Vector2 _scrollPos;
        private readonly ProjectWriter _projectWriter = new();
        private int _ppu = 100;

        public void CreateLoadedPathsPanel()
        {
            GUIStyle rightAlignedStyle = new(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleRight
            };

            _ppu = EditorGUILayout.IntField(ELT("ppu"), _ppu);

            GUILayout.Space(5);

            // Only update when necessary
            if (GetLoadedPaths == null || GetLoadedPathsChanged())
            {
                SetLoadedPaths(LoadPathsFromDB());
                SetLoadedPathsChanged(false);
            }

            // Scroll view for loaded paths
            GUILayout.Label(ELT("loaded_files"), EditorStyles.boldLabel);
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Height(50));
            foreach (var item in GetLoadedPaths())
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.SelectableLabel(item, rightAlignedStyle, GUILayout.Height(15));
                if (GUILayout.Button(ELT("copy"), GUILayout.Width(50), GUILayout.Height(15)))
                {
                    EditorGUIUtility.systemCopyBuffer = item;
                    Debug.Log("Copied: " + item);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            GUILayout.Space(5);
            if (GUILayout.Button(ELT("load_new")))
            {
                LoadNewFile();
            }
        }

        private void LoadNewFile()
        {
            LoadFileInfo? loadInfo = _projectWriter.LoadNewFile(GetProjectName(),
                                        GetAppEnvironment().Wrapper.FileBrowser,
                                        GetAppEnvironment().Wrapper.JsonSerializer,
                                        LoadType.Texture);
            if (loadInfo == null)
                return;

            AttemptToLoadTexture((LoadFileInfo)loadInfo);
            SetLoadedPathsChanged(true);
        }

        private void AttemptToLoadTexture(LoadFileInfo loadInfo)
        {
            string textureDir = PredefinedPaths.PapayaTextureDir;
            string textureSaveDir = Path.Combine(textureDir, GetProjectName(), loadInfo.folder);
            if (!Directory.Exists(textureSaveDir))
                Directory.CreateDirectory(textureSaveDir);
            GetTextureAssetsLoader().LoadTextureAssets(loadInfo, GetProjectName(), textureSaveDir, _ppu);
        }
    }
}