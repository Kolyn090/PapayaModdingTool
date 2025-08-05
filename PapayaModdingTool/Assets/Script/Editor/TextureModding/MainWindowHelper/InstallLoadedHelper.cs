using System;
using System.Collections.Generic;
using System.IO;
using PapayaModdingTool.Assets.Script.DataStruct.FileRead;
using PapayaModdingTool.Assets.Script.Editor.Writer.ProjectUtil;
using PapayaModdingTool.Assets.Script.Editor.Writer.TextureModding;
using PapayaModdingTool.Assets.Script.Misc.AppCore;
using PapayaModdingTool.Assets.Script.Misc.Naming;
using PapayaModdingTool.Assets.Script.Misc.Paths;
using PapayaModdingTool.Assets.Script.Writer.AddressableTools;
using UnityEditor;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.TextureModding.MainWindowHelper
{
    public class InstallLoadedHelper
    {
        public Func<string, string> ELT;
        public Func<AppEnvironment> GetAppEnvironment;
        public Func<List<string>> GetLoadedPaths;
        public Func<string> GetProjectName;
        public Func<TextureAssetsLoader> GetTextureAssetsLoader;

        private BuildTarget _buildPlatform = BuildTarget.StandaloneWindows64;
        private string _installLoadedPath;
        private string _catalogPath;

        public void CreateInstallLoadedPanel()
        {
            _buildPlatform = (BuildTarget)EditorGUILayout.EnumPopup(ELT("build_target"), _buildPlatform);
            GUILayout.Space(5);
            // Patch catalog.json
            EditorGUILayout.BeginHorizontal();
            _catalogPath = EditorGUILayout.TextField(ELT("catalog_patch"), _catalogPath);
            if (GUILayout.Button(ELT("browse"), GUILayout.Width(60)))
            {
                string[] results = GetAppEnvironment().Wrapper.FileBrowser.OpenFilePanel(ELT("search_catalog"), "", new[] { "json" }, false);
                if (results.Length > 0)
                {
                    _catalogPath = results[0];
                }
                else
                {
                    Debug.LogWarning(ELT("failed_finding_catalog"));
                }
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5);
            GUILayout.Label(ELT("install_modified"), EditorStyles.boldLabel);
            _installLoadedPath = EditorGUILayout.TextField("", _installLoadedPath);
            bool isInstallValid = GetLoadedPaths().Contains(_installLoadedPath);
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
                FixSpriteDumps(tags);
                ImportTextures(tags);
                ImportSpriteDumps(tags);
                if (!string.IsNullOrWhiteSpace(_catalogPath) && Directory.Exists(_catalogPath))
                {
                    AddrTool.PatchCrc(_catalogPath);
                }
            }
            EditorGUI.EndDisabledGroup();
        }

        // Assign asset bundle tag for Texture2D / Atlas (in the future)
        // Return the processed tags (which are the names of the built AssetBundles)
        private List<(string, string)> AssignTag()
        {
            List<(string, string)> result = new();

            // Find all existing Texture assets under install loading path
            string textureProjectPath = string.Format(PredefinedPaths.PapayaTextureProjectPath, GetProjectName());
            string[] texturePaths = Directory.GetDirectories(textureProjectPath, "*", SearchOption.TopDirectoryOnly);
            foreach (string path in texturePaths)
            {
                string fileName = Path.GetFileName(path);
                string assetBundleTag = NameMaker.AssetBundleTag(GetProjectName(), fileName, LoadType.Texture);
                result.Add((assetBundleTag, fileName));
                string[] files = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly);
                // TODO: Not all files are tagged. Texture2D / Atlas
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
            string savingPath = PredefinedPaths.AssetBundlesPath;
            AssetBundleBuilder.BuildAllAssetBundles(savingPath, _buildPlatform);
        }

        // Export the Texture from the AssetBundle created by program to External
        private void ExportTextures(List<(string, string)> bundleFileNames)
        {
            // Save to External/Project/Texture/Exported
            // Load from Papaya_Unity/AssetBundles
            string assetBundlesPath = PredefinedPaths.AssetBundlesPath;
            foreach ((string, string) bundleFileName in bundleFileNames)
            {
                (string bundleName, string fileName) = bundleFileName;
                string bundlePath = Path.Combine(assetBundlesPath, bundleName);
                string textureSavePath = string.Format(PredefinedPaths.ExternalFileTextureExportedFolder, GetProjectName(), fileName);
                GetTextureAssetsLoader().LoadTextureOnly(bundlePath, textureSavePath);
            }
        }

        private void ExportOwningDumps(List<(string, string)> bundleFileNames)
        {
            // * Remove everything in the existing owning dumps first! 

            string assetBundlesPath = PredefinedPaths.AssetBundlesPath;
            foreach ((string, string) bundleFileName in bundleFileNames)
            {
                (string bundleName, string fileName) = bundleFileName;
                string owningDumpsFolder = string.Format(PredefinedPaths.ExternalFileTextureOwningDumpFolder, GetProjectName(), fileName);
                PathUtils.DeleteAllContents(owningDumpsFolder);

                string bundlePath = Path.Combine(assetBundlesPath, bundleName);
                GetTextureAssetsLoader().ExportSpriteDumpsOnly(bundlePath, fileName, GetProjectName());
            }
        }

        private void FixSpriteDumps(List<(string, string)> bundleFileNames)
        {
            foreach ((string, string) bundleFileName in bundleFileNames)
            {
                (string _, string fileName) = bundleFileName;
                string owningDumpsFolder = string.Format(PredefinedPaths.ExternalFileTextureOwningDumpFolder, GetProjectName(), fileName);
                string sourceDumpsFolder = string.Format(PredefinedPaths.ExternalFileTextureSourceDumpFolder, GetProjectName(), fileName);
                new SpriteDumpFixer(owningDumpsFolder, sourceDumpsFolder);
            }
        }

        private void ImportTextures(List<(string, string)> bundleFileNames)
        {
            foreach ((string, string) bundleFileName in bundleFileNames)
            {
                (string _, string fileName) = bundleFileName;
                string exportedPath = string.Format(PredefinedPaths.ExternalFileTextureExportedFolder, GetProjectName(), fileName);
                string bundlePath = _installLoadedPath;
                GetTextureAssetsLoader().ImportTexture(bundlePath, exportedPath);
            }
        }

        private void ImportSpriteDumps(List<(string, string)> bundleFileNames)
        {
            // The path-id is in the end of the json dump file
            // Need to split

            // use install loaded file to find the original bundle
            foreach ((string, string) bundleFileName in bundleFileNames)
            {
                (string _, string fileName) = bundleFileName;
                string owningDumpsFolder = string.Format(PredefinedPaths.ExternalFileTextureOwningDumpFolder, GetProjectName(), fileName);
                string bundlePath = _installLoadedPath;
                // import the sprite dumps from owning dumps
                GetTextureAssetsLoader().ImportFixedSpriteDumps(bundlePath, owningDumpsFolder);
            }
        }
    }
}