using System;
using System.Collections.Generic;
using System.IO;
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
                    ["animation"] = data.animation
                };
                objs.Add(new NewtonsoftJsonObject(obj));
            }

            string content = _jsonSerializer.Serialize(objs, true);
            File.WriteAllText(path, content);
        }
    }
}