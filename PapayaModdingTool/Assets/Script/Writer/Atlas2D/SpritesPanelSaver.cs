using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using PapayaModdingTool.Assets.Script.DataStruct.TextureData;
using PapayaModdingTool.Assets.Script.Wrapper.Json;

namespace PapayaModdingTool.Assets.Script.Writer.Atlas2D
{
    public class SpritesPanelSaver
    {
        private readonly IJsonSerializer _jsonSerializer;

        public SpritesPanelSaver(IJsonSerializer jsonSerializer)
        {
            _jsonSerializer = jsonSerializer;
        }

        public void Save(string savePath, List<SpriteButtonData> datas)
        {
            // Save everything except sprite, is selected
            List<IJsonObject> jsonObjects = new();
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
                    ["has_flip_y"] = data.hasFlipY
                };

                jsonObjects.Add(new NewtonsoftJsonObject(obj));
            }

            string content = _jsonSerializer.Serialize(jsonObjects, true);
            File.WriteAllText(savePath, content);
        }
    }
}