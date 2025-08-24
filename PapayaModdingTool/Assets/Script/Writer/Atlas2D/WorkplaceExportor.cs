using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using PapayaModdingTool.Assets.Script.DataStruct.TextureData;
using PapayaModdingTool.Assets.Script.Misc.Paths;
using PapayaModdingTool.Assets.Script.Wrapper.FileBrowser;
using PapayaModdingTool.Assets.Script.Wrapper.Json;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Writer.Atlas2D
{
    public class WorkplaceExportor
    {
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IFileBrowser _fileBrowser;
        private readonly Func<string, string> _ELT;

        public WorkplaceExportor(IJsonSerializer jsonSerializer, IFileBrowser fileBrowser, Func<string, string> ELT)
        {
            _jsonSerializer = jsonSerializer;
            _fileBrowser = fileBrowser;
            _ELT = ELT;
        }

        public void Export(Texture2D workplaceTexture, List<SpriteButtonData> datas)
        {
            string[] folderPaths = _fileBrowser.OpenFolderPanel(_ELT("save_files_to"), "", false);
            if (folderPaths.Length <= 0)
            {
                Debug.Log("Couldn't find path to Folder.");
            }
            else
            {
                string savePath = Path.Combine(folderPaths[0], PredefinedPaths.ExportFolderName);
                if (!Directory.Exists(savePath))
                    Directory.CreateDirectory(savePath);
                SaveTextureAsPng(workplaceTexture, Path.Combine(savePath, "Texture.png"));
                SaveDatasAsJson(datas, Path.Combine(savePath, "Datas.json"));
            }
        }

        private static void SaveTextureAsPng(Texture2D texture, string path)
        {
            byte[] bytes = texture.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
        }

        private void SaveDatasAsJson(List<SpriteButtonData> datas, string path)
        {
            // Sort in this way so that it's easier to get (x, y)
            List<SpriteButtonData> sortedDatas = datas.OrderByDescending(x => x.level)
                                                        .ThenBy(x => x.order).ToList();
            List<int> sortedLevels = GetSortedLevels(sortedDatas);
            Dictionary<int, int> heightForLevels = GetHeightForLevels(sortedDatas);

            List<IJsonObject> objs = new();
            foreach (SpriteButtonData data in datas)
            {
                JObject obj = new()
                {
                    ["label"] = data.label,
                    ["original_label"] = data.originalLabel,
                    ["width"] = data.width,
                    ["height"] = data.height,
                    ["level"] = data.level,
                    ["order"] = data.order,
                    ["pivot_x"] = data.pivot.x,
                    ["pivot_y"] = data.pivot.y,
                    ["animation"] = data.animation,
                    ["x"] = CalculateX(sortedDatas, data.level, data.order),
                    ["y"] = CalculateY(heightForLevels, sortedLevels, data.level, data.height)
                };
                objs.Add(new NewtonsoftJsonObject(obj));
            }

            string content = _jsonSerializer.Serialize(objs, true);
            File.WriteAllText(path, content);
        }

        /// <summary>
        /// Get the highest height for each level
        /// </summary>
        /// <param name="sortedDatas"></param>
        /// <returns></returns>
        private Dictionary<int, int> GetHeightForLevels(List<SpriteButtonData> sortedDatas)
        {
            Dictionary<int, int> result = new();
            foreach (SpriteButtonData data in sortedDatas)
            {
                if (!result.ContainsKey(data.level))
                {
                    result[data.level] = data.height;
                }
                else
                {
                    if (result[data.level] < data.height)
                        result[data.level] = data.height;
                }
            }
            return result;
        }

        private List<int> GetSortedLevels(List<SpriteButtonData> sortedDatas)
        {
            List<int> result = new();
            foreach (SpriteButtonData data in sortedDatas)
            {
                if (!result.Contains(data.level))
                    result.Add(data.level);
            }
            return result;
        }

        private int CalculateX(List<SpriteButtonData> sortedDatas,
                                int level,
                                int order,
                                int gap = 1)
        {
            List<SpriteButtonData> datasInLevel = sortedDatas.Where(x => x.level == level).ToList();
            int result = gap;
            foreach (SpriteButtonData data in datasInLevel)
            {
                if (data.order != order)
                {
                    result += data.width + gap;
                }
                else
                {
                    return result;
                }
            }
            return result;
        }

        private int CalculateY(Dictionary<int, int> heightForLevels,
                                List<int> sortedLevels, // remember, this is descending
                                int level,
                                int height,
                                int gap = 1)
        {
            int GetNextLevel()
            {
                for (int i = 0; i < sortedLevels.Count; i++)
                {
                    if (sortedLevels[i] == level)
                    {
                        return i != 0 ? sortedLevels[i - 1] : -1; // i - 1 because descending
                    }
                }
                return -1; // shouldn't be possible
            }

            int nextLevel = GetNextLevel();
            if (nextLevel == -1) // The provided level is the last
            {
                return gap + heightForLevels[level] - height;
            }
            else
            {
                return gap + heightForLevels[level] - height +
                        heightForLevels[nextLevel] + CalculateY(heightForLevels, sortedLevels, nextLevel, heightForLevels[nextLevel], gap);
            }
        }
    }
}