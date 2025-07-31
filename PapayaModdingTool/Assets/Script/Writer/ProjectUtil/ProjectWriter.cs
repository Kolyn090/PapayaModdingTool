using System.Collections.Generic;
using System.IO;
using PapayaModdingTool.Assets.Script.DataStruct.FileRead;
using PapayaModdingTool.Assets.Script.Misc.Paths;
using PapayaModdingTool.Assets.Script.Wrapper.FileBrowser;
using PapayaModdingTool.Assets.Script.Wrapper.Json;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Writer.ProjectUtil
{
    public class ProjectWriter
    {
        public LoadFileInfo? LoadNewFile(string projectName, IFileBrowser fileBrowser, IJsonSerializer jsonSerializer)
        {
            string[] selections = fileBrowser.OpenFilePanel(
                "Search bundle file",
                "",
                false);
            if (selections.Length <= 0)
            {
                Debug.Log("Couldn't find path to File.");
                return null;
            }
            else
            {
                string selection = PathUtils.GetLongPath(selections[0]);
                string fileFolderName = Path.GetFileName(selection).Replace(".", "_"); // !!! Assume name won't have duplicate

                // Read info.json
                string projectPath = Path.Combine(PredefinedPaths.ProjectsPath, projectName);
                string infoJsonPath = Path.Combine(projectPath, "info.json");

                // Create file folder (the folder for papaya meta files of bundle file)
                string fileFolderPath = Path.Combine(projectPath, fileFolderName);
                if (Directory.Exists(fileFolderPath))
                {
                    // This bundle already has been loaded
                    Debug.LogWarning($"{fileFolderName} has been loaded. Abort.");
                    return null;
                }

                if (!File.Exists(infoJsonPath))
                {
                    Debug.LogError($"info.json not found in {projectName}. This shouldn't happen.");
                    return null;
                }

                string jsonContent = File.ReadAllText(infoJsonPath);
                IJsonObject jsonObject = jsonSerializer.DeserializeToObject(jsonContent);
                List<IJsonObject> currLoaded = jsonObject.GetArray("loaded");

                LoadFileInfo loadFileInfo = new()
                {
                    absolute_path = selection,
                    folder = fileFolderName
                };
                string newJsonContent = jsonSerializer.Serialize(loadFileInfo);
                IJsonObject newJsonObject = jsonSerializer.DeserializeToObject(newJsonContent);
                currLoaded.Add(newJsonObject);
                jsonObject.SetArray("loaded", currLoaded);
                jsonContent = jsonSerializer.SerializeNoFirstLayer(jsonObject);
                File.WriteAllText(infoJsonPath, jsonContent);

                // Create file folder
                string newFileFolderPath = Path.Combine(projectPath, fileFolderName);
                if (!Directory.Exists(newFileFolderPath))
                    Directory.CreateDirectory(newFileFolderPath);

                return loadFileInfo;
            }
        }
    }
}