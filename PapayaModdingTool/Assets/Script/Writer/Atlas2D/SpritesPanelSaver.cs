using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json.Linq;
using PapayaModdingTool.Assets.Script.DataStruct.TextureData;
using PapayaModdingTool.Assets.Script.Reader.Atlas2D;
using PapayaModdingTool.Assets.Script.Wrapper.Json;

namespace PapayaModdingTool.Assets.Script.Writer.Atlas2D
{
    public class SpritesPanelSaver
    {
        private readonly IJsonSerializer _jsonSerializer;
        private readonly SpritesPanelReader _reader;

        public SpritesPanelSaver(IJsonSerializer jsonSerializer)
        {
            _jsonSerializer = jsonSerializer;
            _reader = new(jsonSerializer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="savePath"> The actual path the final content is saved to </param>
        /// <param name="sourcePath"> Used for retrieval </param>
        /// <param name="datas"></param>
        public void Save(string savePath, string sourcePath, List<SpriteButtonData> datas)
        {
            List<IJsonObject> items = _reader.ReadSavePath(savePath);
            int indexOfSourcePath = _reader.GetIndexOfSourcePath(items, sourcePath);

            // Save everything except sprite, is selected
            List<JObject> content = new();
            foreach (SpriteButtonData data in datas)
            {
                JObject obj = new()
                {
                    ["label"] = data.label,
                    ["width"] = data.width,
                    ["height"] = data.height,
                    ["pivot_x"] = data.pivot.x,
                    ["pivot_y"] = data.pivot.y,
                    ["level"] = data.level,
                    ["order"] = data.order,
                    ["animation"] = data.animation,
                    ["has_flip_x"] = data.hasFlipX,
                    ["has_flip_y"] = data.hasFlipY,
                    ["original_label"] = data.originalLabel
                };

                content.Add(obj);
            }
            JObject item = new()
            {
                ["source_path"] = sourcePath
            };
            item["content"] = new JArray(content);

            List<IJsonObject> saveObjects;

            if (indexOfSourcePath == -1)
            {
                saveObjects = items;
                saveObjects ??= new();
                saveObjects.Add(new NewtonsoftJsonObject(item));
            }
            else
            {
                saveObjects = items;
                saveObjects[indexOfSourcePath] = new NewtonsoftJsonObject(item);
            }

            if (!Directory.Exists(Path.GetDirectoryName(savePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(savePath));
            }
            File.WriteAllText(savePath, _jsonSerializer.Serialize(saveObjects, true));
        }
    }
}