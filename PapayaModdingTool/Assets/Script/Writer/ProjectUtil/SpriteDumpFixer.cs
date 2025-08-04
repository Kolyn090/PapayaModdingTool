using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PapayaModdingTool.Assets.Script.Misc.Paths;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.Writer.ProjectUtil
{
    public class SpriteDumpFixer
    {
        private readonly string _owningSpriteDumpsFolderPath = "";
        private readonly string _sourceSpriteDumpsFolderPath = "";

        public SpriteDumpFixer(string owningSpriteDumpsFolderPath,
                                string sourceSpriteDumpsFolderPath)
        {
            _owningSpriteDumpsFolderPath = owningSpriteDumpsFolderPath;
            _sourceSpriteDumpsFolderPath = sourceSpriteDumpsFolderPath;
            FixNamesOfSpriteDumps();
            FixPathID();
        }

        private (string, string) GetBaseNameFullName(string dumpName)
        {
            (string, string) splitCab = PathUtils.SplitByLastRegex(Path.GetFileName(dumpName).Replace(".json", ""), "-CAB-");
            return new(splitCab.Item1, dumpName);
        }

        private void FixNamesOfSpriteDumps()
        {
            int numOfSuccess = 0;
            int numOfError = 0;
            Dictionary<string, string> sourceBaseFullName = new();
            string[] sourceJsonFiles = Directory.GetFiles(_sourceSpriteDumpsFolderPath, "*.json", SearchOption.TopDirectoryOnly);
            foreach (string jsonFile in sourceJsonFiles)
            {
                JObject spriteJson = JObject.Parse(File.ReadAllText(jsonFile));
                if (spriteJson.ContainsKey("m_Rect")) // Make sure it's a Sprite Dump File
                {
                    string fileName = Path.GetFileName(jsonFile);
                    (string, string) baseNameFullName = GetBaseNameFullName(fileName);
                    string baseName = baseNameFullName.Item1;
                    string fullName = baseNameFullName.Item2;
                    sourceBaseFullName.Add(baseName, fullName);
                }
            }

            string[] owningJsonFiles = Directory.GetFiles(_owningSpriteDumpsFolderPath, "*.json", SearchOption.TopDirectoryOnly);
            foreach (string jsonFile in owningJsonFiles)
            {
                JObject spriteJson = JObject.Parse(File.ReadAllText(jsonFile));
                if (spriteJson.ContainsKey("m_Rect")) // Make sure it's a Sprite Dump File
                {
                    string fileName = Path.GetFileName(jsonFile);
                    (string, string) baseNameFullName = GetBaseNameFullName(fileName);
                    string baseName = baseNameFullName.Item1;
                    if (sourceBaseFullName.ContainsKey(baseName))
                    {
                        numOfSuccess++;
                        string newName = sourceBaseFullName[baseName];
                        string newPath = Path.Combine(Path.GetDirectoryName(jsonFile), newName);
                        File.Move(jsonFile, newPath);
                    }
                    else
                    {
                        numOfError++;
                        Debug.LogError($"Missing Source Sprite Dump File for {baseName}.");
                    }
                }
            }

            Debug.Log($"Fixed {numOfSuccess} names of Sprite Dump Files in {_owningSpriteDumpsFolderPath}. Found {numOfError} error(s).");
        }

        private void FixPathID()
        {
            string[] sourceJsonFiles = Directory.GetFiles(_sourceSpriteDumpsFolderPath, "*.json", SearchOption.TopDirectoryOnly);
            long? replacePathID = null;
            foreach (string jsonFile in sourceJsonFiles)
            {
                JObject spriteJson = JObject.Parse(File.ReadAllText(jsonFile));
                if (spriteJson.ContainsKey("m_Rect")) // Make sure it's a Sprite Dump File
                {
                    replacePathID = long.Parse(spriteJson["m_RD"]["texture"]["m_PathID"].ToString());
                    break;
                }
            }

            if (replacePathID == null)
            {
                Debug.LogError($"No Source Sprite Dump File found in {_sourceSpriteDumpsFolderPath}");
                return;
            }

            int numOfSuccess = 0;
            string[] jsonFiles = Directory.GetFiles(_owningSpriteDumpsFolderPath, "*.json", SearchOption.TopDirectoryOnly);
            foreach (string jsonFile in jsonFiles)
            {
                JObject spriteJson = JObject.Parse(File.ReadAllText(jsonFile));
                if (spriteJson.ContainsKey("m_Rect")) // Make sure it's a Sprite Dump File
                {
                    spriteJson["m_RD"]["texture"]["m_PathID"] = replacePathID;
                    numOfSuccess++;
                }
                File.WriteAllText(jsonFile, JsonConvert.SerializeObject(spriteJson, Formatting.Indented));
            }

            Debug.Log($"Fixed Path ID of {numOfSuccess} Sprite Dump Files in {_owningSpriteDumpsFolderPath}.");
        }
    }
}