using System;
using System.Collections.Generic;
using System.IO;
using PapayaModdingTool.Assets.Script.DataStruct.FileRead;
using PapayaModdingTool.Assets.Script.Misc.Paths;
using PapayaModdingTool.Assets.Script.Reader.ProjectUtil;
using PapayaModdingTool.Assets.Script.Wrapper.Json;
using UnityEditor;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.Writer.Universal
{
    public class ProjectRemover
    {
        private readonly IJsonSerializer _jsonSerializer;

        public ProjectRemover(IJsonSerializer jsonSerializer)
        {
            _jsonSerializer = jsonSerializer;
        }

        public void RemoveProject(string projectName, Func<string, string> ELT)
        {
            ProjectLoader projectLoader = new();
            List<string> filePaths = projectLoader.FindLoadedPaths(projectName, _jsonSerializer);
            foreach (string path in filePaths)
            {
                foreach (LoadType loadType in Enum.GetValues(typeof(LoadType)))
                {
                    RemoveFileFromProject(path, projectName, loadType);
                }
            }

            // ! Remove the paths in unity - Texture
            string textureProjectPath = string.Format(PredefinedPaths.PapayaTextureProjectPath, projectName);
            if (Directory.Exists(textureProjectPath))
                Directory.Delete(textureProjectPath);
            string textureProjectMeta = textureProjectPath + ".meta";
            if (File.Exists(textureProjectMeta))
                File.Delete(textureProjectMeta);
            
            string projectPath = Path.Combine(PredefinedPaths.ProjectsPath, projectName);
            if (Directory.Exists(projectPath))
            {
                try
                {
                    Directory.Delete(projectPath, true);
                    if (File.Exists(projectPath + ".meta"))
                        File.Delete(projectPath + ".meta");

                    EditorUtility.DisplayDialog(
                        ELT("project_deleted_title"),
                        string.Format(ELT("project_deleted_message"), Path.GetFileName(projectPath)),
                        ELT("ok")
                    );
                }
                catch (Exception ex)
                {
                    Debug.LogError($"{ELT("fail_to_delete_project")}: " + ex.Message);
                    EditorUtility.DisplayDialog(ELT("error"), ELT("fail_to_delete_project"), ELT("ok"));
                }
            }
        }

        public void RemoveFileFromProject(string removeLoadedPath, string projectName, LoadType loadType)
        {
            // Eliminate all traces of the specified file from the current project (Texture)
            // From Unity
            string _removeLoadedFileName = Path.GetFileName(removeLoadedPath).Replace(".", "_");
            string _textureUnityFolder = Path.Combine(PredefinedPaths.PapayaUnityDir,
                                                        loadType.ToString(),
                                                        projectName);
            string _removeFolderInUnity = Path.Combine(_textureUnityFolder, _removeLoadedFileName);
            if (Directory.Exists(_removeFolderInUnity))
            {
                PathUtils.DeleteAllContents(_removeFolderInUnity);
                Directory.Delete(_removeFolderInUnity);
                File.Delete(_removeFolderInUnity + ".meta");
                AssetDatabase.Refresh();
            }
            else
            {
                Debug.LogWarning($"Missing {_removeFolderInUnity}. Abort.");
            }

            // From External
            string _removeFolderInProject = Path.Combine(PredefinedPaths.ProjectsPath,
                                                        projectName,
                                                        _removeLoadedFileName);
            if (Directory.Exists(_removeFolderInProject))
            {
                PathUtils.DeleteAllContents(_removeFolderInProject);
                Directory.Delete(_removeFolderInProject);
                AssetDatabase.Refresh();
            }
            else
            {
                Debug.LogWarning($"Missing {_removeFolderInProject}. Abort.");
            }

            // From info.json (make sure you only delete the one with type Texture)
            string projectPath = Path.Combine(PredefinedPaths.ProjectsPath, projectName);
            string infoJsonPath = Path.Combine(projectPath, "info.json");
            if (!File.Exists(infoJsonPath))
            {
                Debug.LogError($"info.json not found in {projectName}. This shouldn't happen.");
                return;
            }

            string jsonContent = File.ReadAllText(infoJsonPath);
            IJsonObject jsonObject = _jsonSerializer.DeserializeToObject(jsonContent);
            List<IJsonObject> currLoaded = jsonObject.GetArray("loaded");
            int indexToRemove = -1;
            for (int i = 0; i < currLoaded.Count; i++)
            {
                IJsonObject loaded = currLoaded[i];
                if (loaded.GetString("folder") == _removeLoadedFileName && loaded.GetString("type") == loadType.ToString())
                {
                    indexToRemove = i;
                    break;
                }
            }
            if (indexToRemove != -1)
                currLoaded.RemoveAt(indexToRemove);

            jsonObject.SetArray("loaded", currLoaded);
            jsonContent = _jsonSerializer.SerializeNoFirstLayer(jsonObject);
            File.WriteAllText(infoJsonPath, jsonContent);

            // From AssetBundle
            string assetBundlePath = Path.Combine(PredefinedPaths.PapayaUnityDir, "AssetBundles");
            string removePrefix = projectName + "_" + loadType.ToString().ToLower();
            var files = Directory.GetFiles(assetBundlePath);

            foreach (var filePath in files)
            {
                string fileName = Path.GetFileName(filePath);
                if (fileName.StartsWith(removePrefix)) // TODO: this will not work in case of 'abc' and 'abc2', delete 1 will also delete 2
                {
                    try
                    {
                        File.Delete(filePath);
                        Debug.Log($"Deleted file: {filePath}");
                    }
                    catch (IOException ex)
                    {
                        Debug.LogError($"Failed to delete {filePath}: {ex.Message}");
                    }
                }
            }
        }
    }
}
