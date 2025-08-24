using System;
using System.IO;
using PapayaModdingTool.Assets.Script.__Test__.TestUtil;
using PapayaModdingTool.Assets.Script.Editor.Universal;
using PapayaModdingTool.Assets.Script.Editor.Writer.ProjectUtil;
using UnityEditor;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.EditorTimeTest
{
    public class TestAssetBundle : BaseEditorWindow
    {
        [MenuItem("Tools/__ TestAssetBundle", false, 99)]
        public static void ShowWindow()
        {
            Initialize();
            GetWindow<TestAssetBundle>("Asset Bundle Tests");
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox(ELT("dev_only"), MessageType.Warning);

            GUILayout.Label("Don't forget to refresh.", EditorStyles.boldLabel);

            GUILayout.Space(20);

            if (GUILayout.Button("Test Build Asset Bundles"))
            {
                TestBuildAssetBundles(() =>
                {
                    Debug.Log($"Test Build Asset Bundles is done");
                    string assetBundlesPath = Path.Combine(PredefinedTestPaths.UnityLabDeskPath, "AssetBundles");
                    string texturePath = Path.Combine(assetBundlesPath, "Texture");
                    Debug.Log($"Check {texturePath} to verify new bundle.");
                });
            }

            GUILayout.Space(20);

            if (GUILayout.Button("Test Assign Asset Bundle Tag"))
            {
                TestAssetBundleTag(() =>
                {
                    Debug.Log($"Test Asset Bundle Tag is done");
                    string texturePath = Path.Combine(PredefinedTestPaths.UnityLabDeskPath, "Texture");
                    string targetAssetPath = Path.Combine(texturePath, "alchemist_0.png");
                    string BUNDLE_NAME = "alchemist";
                    Debug.Log($"Check {targetAssetPath} to verify it asset bundle tag is {BUNDLE_NAME}.");
                });
            }
        }

        public void TestAssetBundleTag(Action onComplete)
        {
            TestOnCleanLabDesk(() =>
            {
                string texturePath = Path.Combine(PredefinedTestPaths.UnityLabDeskPath, "Texture");
                string targetAssetPath = Path.Combine(texturePath, "alchemist_0.png");

                string BUNDLE_NAME = "alchemist";
                AssetBundleTagEditor.SetAssetBundleTag(targetAssetPath, BUNDLE_NAME);
                onComplete?.Invoke();
            },
            PredefinedTestPaths.UnityDoNotOverridePath,
            PredefinedTestPaths.UnityLabDeskPath,
            true, true);
        }

        public void TestBuildAssetBundles(Action onComplete)
        {
            // ! Only clean, don't copy (to avoid duplicate assets)
            TestOnCleanLabDesk(() =>
            {
                string assetBundlesPath = Path.Combine(PredefinedTestPaths.UnityLabDeskPath, "AssetBundles");
                string texturePath = Path.Combine(assetBundlesPath, "Texture");

                AssetBundleBuilder.BuildAllAssetBundles(texturePath,
                                                        BuildTarget.StandaloneWindows64);
                onComplete?.Invoke();
            },
            PredefinedTestPaths.UnityDoNotOverridePath,
            PredefinedTestPaths.UnityLabDeskPath,
            true, false);
        }

        public static void TestOnCleanLabDesk(Action onCleanComplete,
                                                string doNotOverwritePath = null,
                                                string labDeskPath = null,
                                                bool isUnityFolder = false,
                                                bool rebuild = false)
        {
            doNotOverwritePath ??= PredefinedTestPaths.DoNotOverridePath;
            labDeskPath ??= PredefinedTestPaths.LabDeskPath;

            SafeClearDirectoryContents(labDeskPath);

            if (rebuild)
            {
                if (!isUnityFolder)
                {
                    CopyDirectory(doNotOverwritePath, labDeskPath, onCleanComplete);
                }
                else
                {
                    CopyUnityDirectory(doNotOverwritePath, labDeskPath, onCleanComplete);
                }
            }
            onCleanComplete?.Invoke();
        }

        private static void CopyDirectory(string sourceDir, string targetDir, Action onComplete)
        {
            if (!Directory.Exists(sourceDir))
            {
                Debug.Log("Source directory does not exist: " + sourceDir);
                return;
            }

            Directory.CreateDirectory(targetDir);

            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string destSubDir = Path.Combine(targetDir, Path.GetFileName(subDir));
                CopyDirectory(subDir, destSubDir, null);
            }

            foreach (string file in Directory.GetFiles(sourceDir))
            {
                if (file.EndsWith(".meta")) continue; // Skip meta files!
                string destFile = Path.Combine(targetDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }

            onComplete?.Invoke();
        }

        private static void SafeClearDirectoryContents(string rootPath)
        {
            try
            {
                if (!Directory.Exists(rootPath))
                    return;

                // Delete all files in the root and subfolders
                foreach (var file in Directory.GetFiles(rootPath, "*", SearchOption.TopDirectoryOnly))
                {
                    SafeDeleteFile(file);
                }

                // Delete all immediate subdirectories
                foreach (var dir in Directory.GetDirectories(rootPath, "*", SearchOption.TopDirectoryOnly))
                {
                    SafeDeleteDirectory(dir);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to clear directory contents of '{rootPath}': {ex.Message}");
            }
        }

        private static void SafeDeleteDirectory(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                    return;

                // Remove read-only flags
                foreach (var file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
                    File.SetAttributes(file, FileAttributes.Normal);

                Directory.Delete(path, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to delete {path}: {ex.Message}");
            }
        }

        private static void SafeDeleteFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return;

                // Remove read-only or system attribute
                var attributes = File.GetAttributes(filePath);
                if ((attributes & FileAttributes.ReadOnly) != 0 || (attributes & FileAttributes.System) != 0)
                {
                    File.SetAttributes(filePath, FileAttributes.Normal);
                }

                File.Delete(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to delete file '{filePath}': {ex.Message}");
            }
        }

        public static void CopyUnityDirectory(string sourceDir, string targetDir, Action onComplete)
        {
            if (!Directory.Exists(sourceDir))
            {
                Debug.LogError("Source dir does not exist: " + sourceDir);
                return;
            }

            CopyAll(new DirectoryInfo(sourceDir), new DirectoryInfo(targetDir));

            // Convert full paths to relative Unity paths for refresh
            AssetDatabase.Refresh();
            onComplete?.Invoke();
        }

        private static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            // Copy all files (including .meta)
            foreach (FileInfo fi in source.GetFiles())
            {
                string destFile = Path.Combine(target.FullName, fi.Name);
                fi.CopyTo(destFile, true);
            }

            // Recursively copy subfolders
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }
    }
}