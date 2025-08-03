using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.Writer.TextureModding
{
    public class SpritesheetFromDump
    {
        private readonly struct RenderDataKey
        {
            readonly uint firstData0;
            readonly uint firstData1;
            readonly uint firstData2;
            readonly uint firstData3;
            readonly long second;
            public RenderDataKey(uint firstData0, uint firstData1, uint firstData2, uint firstData3, long second)
            {
                this.firstData0 = firstData0;
                this.firstData1 = firstData1;
                this.firstData2 = firstData2;
                this.firstData3 = firstData3;
                this.second = second;
            }

            public bool Compare(RenderDataKey other)
            {
                return firstData0 == other.firstData0 &&
                    firstData1 == other.firstData1 &&
                    firstData2 == other.firstData2 &&
                    firstData3 == other.firstData3 &&
                    second == other.second;
            }

            public override string ToString()
            {
                return $"\"first\"[{firstData0}, {firstData1}, {firstData2}, {firstData3}], \"second\": {second}";
            }
        }

        private readonly Texture2D _targetTexture;
        private readonly string _sourceDumpsFolderPath;
        private readonly string _sourceAtlasFilePath; // if used
        private readonly int _gamePPU;

        public SpritesheetFromDump(Texture2D targetTexture, string sourceDumpsFolderPath,
                                    string sourceAtlasFilePath="", int gamePPU=100)
        {
            _targetTexture = targetTexture;
            _sourceDumpsFolderPath = sourceDumpsFolderPath;
            _sourceAtlasFilePath = sourceAtlasFilePath;
            _gamePPU = gamePPU;
        }

        private string GetSourceAtlasFilePath()
        {
            if (!string.IsNullOrWhiteSpace(_sourceAtlasFilePath))
            {
                return _sourceAtlasFilePath;
            }
            string[] jsonFiles = Directory.GetFiles(_sourceDumpsFolderPath, "*.json", SearchOption.TopDirectoryOnly);
            foreach (string jsonFile in jsonFiles)
            {
                JObject spriteJson = JObject.Parse(File.ReadAllText(jsonFile));
                if (spriteJson.ContainsKey("m_PackedSprites")) // is SpriteAtlas Dump File
                {
                    return jsonFile;
                }
            }
            return "";
        }

        // Atlas
        private Dictionary<int, long> GetIndex2PathID()
        {
            Dictionary<int, long> result = new();
            JObject sourceAtlasFileJson = JObject.Parse(File.ReadAllText(_sourceAtlasFilePath));
            var packedSpritesSource = sourceAtlasFileJson["m_PackedSprites"]["Array"];
            for (int i = 0; i < packedSpritesSource.Count(); i++)
            {
                var pathID = long.Parse(packedSpritesSource[i]["m_PathID"].ToString());
                result[i] = pathID;
            }
            return result;
        }

        private (string, long) GetNamePathIDFromDumpName(string dumpName)
        {
            string[] splitCab = Path.GetFileName(dumpName).Replace(".json", "").Split("-CAB-");
            string name = splitCab[0];
            string[] splitted = splitCab[1].Split("-");
            long absPathID = long.Parse(splitted[splitted.Length - 1].ToString());
            long pathID = absPathID;
            if (splitted.Length == 3)
            {  // This Path ID is negative
                pathID = -absPathID;
            }
            else if (splitted.Length != 2)
            {
                Debug.LogError($"{name} does not fit format.");
                pathID = -1;
            }
            return new(name, pathID);
        }

        // Atlas
        private Dictionary<long, int> GetPathID2Index()
        {
            Dictionary<int, long> index2PathID = GetIndex2PathID();
            return index2PathID.ToDictionary(pair => pair.Value, pair => pair.Key);
        }

        // Atlas
        private Dictionary<int, RenderDataKey> GetIndex2RenderDataKey()
        {
            Dictionary<long, int> pathID2Index = GetPathID2Index();
            Dictionary<int, RenderDataKey> result = new();
            string[] jsonFiles = Directory.GetFiles(_sourceDumpsFolderPath, "*.json", SearchOption.TopDirectoryOnly);
            foreach (string jsonFile in jsonFiles)
            {
                string jsonFileName = Path.GetFileName(jsonFile);
                (string, long) namePathId = GetNamePathIDFromDumpName(jsonFileName);
                long pathID = namePathId.Item2;
                JObject spriteJson = JObject.Parse(File.ReadAllText(jsonFile));
                if (spriteJson.ContainsKey("m_Rect"))
                {
                    var rdk = spriteJson["m_RenderDataKey"];
                    uint firstData0 = uint.Parse(rdk["first"]["data[0]"].ToString());
                    uint firstData1 = uint.Parse(rdk["first"]["data[1]"].ToString());
                    uint firstData2 = uint.Parse(rdk["first"]["data[2]"].ToString());
                    uint firstData3 = uint.Parse(rdk["first"]["data[3]"].ToString());
                    long second = long.Parse(rdk["second"].ToString());
                    RenderDataKey renderDataKey = new(firstData0, firstData1, firstData2, firstData3, second);
                    result[pathID2Index[pathID]] = renderDataKey;
                }
            }
            return result;
        }

        // Atlas
        private int SearchIndexOfRenderDataKey(List<RenderDataKey> lst, RenderDataKey target)
        {
            int counter = 0;
            foreach (RenderDataKey rdk in lst)
            {
                if (rdk.Compare(target))
                {
                    return counter;
                }
                counter++;
            }
            return -1;
        }

        // Atlas
        private List<RenderDataKey> GetRenderDataKeysFromJObject(JObject jObject)
        {
            var renderDataMap = jObject["m_RenderDataMap"]["Array"];
            List<RenderDataKey> renderDataKeys = new();
            for (int i = 0; i < renderDataMap.Count(); i++)
            {
                var rdk = renderDataMap[i];
                uint firstData0 = uint.Parse(rdk["first"]["first"]["data[0]"].ToString());
                uint firstData1 = uint.Parse(rdk["first"]["first"]["data[1]"].ToString());
                uint firstData2 = uint.Parse(rdk["first"]["first"]["data[2]"].ToString());
                uint firstData3 = uint.Parse(rdk["first"]["first"]["data[3]"].ToString());
                long second = long.Parse(rdk["first"]["second"].ToString());
                RenderDataKey renderDataKey = new(firstData0, firstData1, firstData2, firstData3, second);
                renderDataKeys.Add(renderDataKey);
            }
            return renderDataKeys;
        }

        // Atlas
        private Dictionary<int, int> GetIndex2ActualRenderDataKeyIndex()
        {
            Dictionary<int, int> result = new();
            Dictionary<int, RenderDataKey> index2RenderDataKey = GetIndex2RenderDataKey();
            JObject sourceAtlasFileJson = JObject.Parse(File.ReadAllText(_sourceAtlasFilePath));
            List<RenderDataKey> renderDataKeys = GetRenderDataKeysFromJObject(sourceAtlasFileJson);

            foreach (int index in index2RenderDataKey.Keys)
            {
                if (index2RenderDataKey.TryGetValue(index, out RenderDataKey rdk))
                {
                    result[index] = SearchIndexOfRenderDataKey(renderDataKeys, rdk);
                }
            }

            return result;
        }

        public void Import()
        {
            List<SpriteMetaData> metas = MetasFromDumps();
            SpriteMetaData[] newMetas = metas.ToArray();
            string assetPath = AssetDatabase.GetAssetPath(_targetTexture);
            TextureImporter ti = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (ti != null)
            {
                ti = AutoConfigureInspector(ti);
                ti.spriteImportMode = SpriteImportMode.Multiple;
                ti.spritesheet = newMetas;
                Debug.Log($"Current number of Sprites in the sheet: {metas.Count()}.");
                EditorUtility.SetDirty(ti);
                ti.SaveAndReimport();
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                CleanMetaNameFileIdTable(assetPath);
            }
            else
            {
                Debug.LogError($"Texture not found in {_targetTexture}.");
            }
        }

        private TextureImporter AutoConfigureInspector(TextureImporter ti)
        {
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Multiple;

            TextureImporterSettings textureSettings = new();
            ti.ReadTextureSettings(textureSettings);
            textureSettings.spriteMeshType = SpriteMeshType.FullRect;
            textureSettings.spriteGenerateFallbackPhysicsShape = false;

            ti.SetTextureSettings(textureSettings);
            ti.isReadable = true;
            ti.alphaIsTransparency = true;
            ti.filterMode = FilterMode.Point;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti = ChangeTexturePPU(ti);

            return ti;
        }

        private TextureImporter ChangeTexturePPU(TextureImporter ti)
        {
            int newPPU = _gamePPU;
            ti.spritePixelsPerUnit = newPPU;
            return ti;
        }

        private List<SpriteMetaData> MetasFromDumps()
        {
            // If atlas is used, read texture rect from Atlas Dump.
            // Otherwise, read both texture rect and offset (pivot) from Sprite Dumps.
            List<SpriteMetaData> result = new();
            Dictionary<int, int> index2ActualRenderDataKeyIndex = null;
            Dictionary<long, int> pathID2Index = null;
            JToken renderDataMaps = null;

            if (!string.IsNullOrWhiteSpace(_sourceAtlasFilePath))
            {
                index2ActualRenderDataKeyIndex = GetIndex2ActualRenderDataKeyIndex();
                pathID2Index = GetPathID2Index();
                JObject sourceAtlasFileJson = JObject.Parse(File.ReadAllText(_sourceAtlasFilePath));
                renderDataMaps = sourceAtlasFileJson["m_RenderDataMap"]["Array"];
            }

            int counter = 0;
            HashSet<string> seenNames = new();
            string[] jsonFiles = Directory.GetFiles(_sourceDumpsFolderPath, "*.json", SearchOption.TopDirectoryOnly);
            foreach (string jsonFile in jsonFiles)
            {
                JObject spriteJson = JObject.Parse(File.ReadAllText(jsonFile));
                if (!spriteJson.ContainsKey("m_Rect")) // Not a Sprite Dump File
                    continue;
                (string, long) namePathID = GetNamePathIDFromDumpName(jsonFile);
                string name = namePathID.Item1;
                if (seenNames.Contains(name))
                {
                    name = name + " #" + counter.ToString();
                }
                counter++;
                seenNames.Add(name);
                long pathID = namePathID.Item2;
                float pivotX = float.Parse(spriteJson["m_Pivot"]["x"].ToString());
                float pivotY = float.Parse(spriteJson["m_Pivot"]["y"].ToString());
                float x;
                float y;
                float width;
                float height;
                if (string.IsNullOrWhiteSpace(_sourceAtlasFilePath))
                {
                    x = float.Parse(spriteJson["m_Rect"]["x"].ToString());
                    y = float.Parse(spriteJson["m_Rect"]["y"].ToString());
                    width = float.Parse(spriteJson["m_Rect"]["width"].ToString());
                    height = float.Parse(spriteJson["m_Rect"]["height"].ToString());
                }
                else
                {  // Is using Atlas
                    int indexInAtlas = pathID2Index[pathID];
                    int actualRenderDataKeyIndex = index2ActualRenderDataKeyIndex[indexInAtlas];
                    var renderDataMapSource = renderDataMaps[actualRenderDataKeyIndex];
                    x = float.Parse(renderDataMapSource["second"]["textureRect"]["x"].ToString());
                    y = float.Parse(renderDataMapSource["second"]["textureRect"]["y"].ToString());
                    width = float.Parse(renderDataMapSource["second"]["textureRect"]["width"].ToString());
                    height = float.Parse(renderDataMapSource["second"]["textureRect"]["height"].ToString());
                }

                SpriteMetaData newMeta = new()
                {
                    name = name,
                    rect = new(x, y, width, height),
                    pivot = new(pivotX, pivotY),
                    border = new(
                        float.Parse(spriteJson["m_Border"]["x"].ToString()),
                        float.Parse(spriteJson["m_Border"]["y"].ToString()),
                        float.Parse(spriteJson["m_Border"]["z"].ToString()),
                        float.Parse(spriteJson["m_Border"]["w"].ToString())
                    ),
                    alignment = (int)SpriteAlignment.Custom
                };
                Debug.Log($"Loaded new Sprite {name}.");

                result.Add(newMeta);
            }

            return result;
        }

        private static void CleanMetaNameFileIdTable(string assetPath)
        {
            string metaPath = assetPath + ".meta";

            if (!File.Exists(metaPath))
            {
                Debug.LogError("Meta file does not exist: " + metaPath);
                return;
            }

            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null || importer.spriteImportMode != SpriteImportMode.Multiple)
            {
                Debug.LogError("TextureImporter not valid or not in Multiple mode.");
                return;
            }

            // Get current sprite names
            HashSet<string> validNames = new();
            foreach (var meta in importer.spritesheet)
            {
                validNames.Add(meta.name);
            }

            // Read and process meta file lines
            string[] lines = File.ReadAllLines(metaPath);
            List<string> newLines = new();
            bool insideTable = false;
            int removed = 0;

            foreach (var line in lines)
            {
                if (line.Trim() == "nameFileIdTable:")
                {
                    insideTable = true;
                    newLines.Add(line);
                    continue;
                }

                if (insideTable)
                {
                    Match match = Regex.Match(line, @"^\s+(.+?):\s*(-?\d+)");
                    if (match.Success)
                    {
                        string key = match.Groups[1].Value;
                        if (validNames.Contains(key))
                            newLines.Add(line);
                        else
                            removed++;
                    }
                    else if (line.StartsWith("  ")) // Still possibly inside block
                    {
                        newLines.Add(line);
                    }
                    else
                    {
                        insideTable = false;
                        newLines.Add(line);
                    }
                }
                else
                {
                    newLines.Add(line);
                }
            }

            File.WriteAllLines(metaPath, newLines);
            Debug.Log($"Cleaned {removed} dangling entries from nameFileIdTable in: {metaPath}");

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        }
    }
}