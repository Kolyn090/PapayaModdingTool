using System.Collections.Generic;
using System.IO;
using PapayaModdingTool.Assets.Script.Wrapper.Json;

namespace PapayaModdingTool.Assets.Script.Writer.ProjectUtil
{
    public class NewProjectCreator
    {
        public void CreateNewProject(string newProjectPath, IJsonSerializer jsonSerializer)
        {
            // Create the directory
            // Expected to be under papaya projects
            Directory.CreateDirectory(newProjectPath);


            // Create info.json
            Dictionary<string, string> jsonContent = new()
            {
                ["project_name"] = Path.GetFileName(newProjectPath)
            };
            string serializedJson = jsonSerializer.Serialize(jsonContent, true);
            string infoJsonPath = Path.Combine(newProjectPath, "info.json");
            File.WriteAllText(infoJsonPath, serializedJson);
        }
    }
}