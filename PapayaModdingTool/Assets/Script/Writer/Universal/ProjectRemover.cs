using System.Collections.Generic;
using System.IO;
using PapayaModdingTool.Assets.Script.DataStruct.FileRead;
using PapayaModdingTool.Assets.Script.Misc.Paths;
using PapayaModdingTool.Assets.Script.Wrapper.Json;
using UnityEditor;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Writer.Universal
{
    public class ProjectRemover
    {
        private readonly IJsonSerializer _jsonSerializer;

        public ProjectRemover(IJsonSerializer jsonSerializer)
        {
            _jsonSerializer = jsonSerializer;
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
            PathUtils.DeleteAllContents(_removeFolderInUnity);
            Directory.Delete(_removeFolderInUnity);
            File.Delete(_removeFolderInUnity + ".meta");
            AssetDatabase.Refresh();

            // From External
            string _textureProjectFolder = Path.Combine(PredefinedPaths.ProjectsPath,
                                                        projectName,
                                                        loadType.ToString());
            string _removeFolderInProject = Path.Combine(_textureProjectFolder, _removeLoadedFileName);
            PathUtils.DeleteAllContents(_removeFolderInProject);
            Directory.Delete(_removeFolderInProject);
            AssetDatabase.Refresh();

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
        }
    }
}