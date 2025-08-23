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
            List<IJsonObject> loaded = FindLoaded(projectName, jsonSerializer);
            return loaded.Select(x => PathUtils.ToLongPath(x.GetString("absolute_path"))).ToList();
        }

        public List<string> FindLoadedFileFolderNames(string projectName, IJsonSerializer jsonSerializer)
        {
            List<IJsonObject> loaded = FindLoaded(projectName, jsonSerializer);
            return loaded.Select(x => PathUtils.ToLongPath(x.GetString("folder"))).ToList();
        }

        public List<string> FindLoadedPathsTextureOnly(string projectName, IJsonSerializer jsonSerializer)
        {
            List<IJsonObject> loaded = FindLoaded(projectName, jsonSerializer);
            return loaded.Where(x => x.GetString("type") == "Texture")
                        .Select(x => PathUtils.ToLongPath(x.GetString("absolute_path"))).ToList();
        }

        public List<string> FindLoadedFileFolderNamesTextureOnly(string projectName, IJsonSerializer jsonSerializer)
        {
            List<IJsonObject> loaded = FindLoaded(projectName, jsonSerializer);
            return loaded.Where(x => x.GetString("type") == "Texture")
                    .Select(x => x.GetString("folder")).ToList();
        }

        public List<(string, string)> FindLoadedPathAndFileFolderNameTextureOnly(string projectName, IJsonSerializer jsonSerializer)
        {
            List<IJsonObject> loaded = FindLoaded(projectName, jsonSerializer);
            return loaded.Where(x => x.GetString("type") == "Texture")
                    .Select(x => (x.GetString("absolute_path"), x.GetString("folder"))).ToList();
        }

        private List<IJsonObject> FindLoaded(string projectName, IJsonSerializer jsonSerializer)
        {
            string infoJsonPath = string.Format(PredefinedPaths.ProjectInfoPath, projectName);

            if (!File.Exists(infoJsonPath))
            {
                UnityEngine.Debug.LogError($"info.json not found in {projectName}. This shouldn't happen.");
                return new();
            }

            string jsonContent = File.ReadAllText(infoJsonPath);
            IJsonObject jsonObject = jsonSerializer.DeserializeToObject(jsonContent);
            return jsonObject.GetArray("loaded"); ;
        }
    }
}