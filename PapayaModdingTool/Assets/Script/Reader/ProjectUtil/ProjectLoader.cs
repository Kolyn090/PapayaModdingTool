using System.Collections.Generic;
using System.IO;
using System.Linq;
using PapayaModdingTool.Assets.Script.Misc.Paths;
using PapayaModdingTool.Assets.Script.Wrapper.Json;

namespace PapayaModdingTool.Assets.Script.Reader.ProjectUtil
{
    public class ProjectLoader
    {
        public List<string> FindLoadedPaths(string projectName, IJsonSerializer jsonSerializer)
        {
            string projectPath = Path.Combine(PredefinedPaths.ProjectsPath, projectName);
            string infoJsonPath = Path.Combine(projectPath, "info.json");

            if (!File.Exists(infoJsonPath))
            {
                UnityEngine.Debug.LogError($"info.json not found in {projectName}. This shouldn't happen.");
                return new();
            }

            string jsonContent = File.ReadAllText(infoJsonPath);
            IJsonObject jsonObject = jsonSerializer.DeserializeToObject(jsonContent);
            List<IJsonObject> loaded = jsonObject.GetArray("loaded");
            return loaded.Select(x => x.GetString("absolute_path")).ToList();
        }
    }
}