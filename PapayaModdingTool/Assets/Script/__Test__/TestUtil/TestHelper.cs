using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.__Test__.TestUtil
{
    public class TestHelper
    {
        public static void TestOnCleanLabDesk(Action onCleanComplete,
                                                string doNotOverwritePath = null,
                                                string labDeskPath = null,
                                                bool isUnityFolder = false)
        {
            doNotOverwritePath ??= PredefinedTestPaths.DoNotOverridePath;
            labDeskPath ??= PredefinedTestPaths.LabDeskPath;

            SafeClearDirectoryContents(labDeskPath);
            if (!isUnityFolder)
            {
                CopyDirectory(doNotOverwritePath, labDeskPath, onCleanComplete);
            }
            else
            {
                CopyUnityDirectory(doNotOverwritePath, labDeskPath, onCleanComplete);
            }
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
    }
}