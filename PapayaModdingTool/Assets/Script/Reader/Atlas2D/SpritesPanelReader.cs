using System.Collections.Generic;
using System.IO;
using PapayaModdingTool.Assets.Script.DataStruct.TextureData;
using PapayaModdingTool.Assets.Script.Wrapper.Json;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Reader.Atlas2D
{
    public class SpritesPanelReader
    {
        private readonly IJsonSerializer _jsonSerializer;

        public SpritesPanelReader(IJsonSerializer jsonSerializer)
        {
            _jsonSerializer = jsonSerializer;
        }

        public List<SpriteButtonData> LoadDatas(List<SpriteButtonData> datas, string savePath, string sourcePath)
        {
            List<SpriteButtonData> savedDatas = GetSavedDatas(savePath, sourcePath);

            // Use original label to load data
            foreach (SpriteButtonData data in datas)
            {
                SpriteButtonData savedData = GetSavedData(savedDatas, data.originalLabel);
                if (savedData == null)
                {
                    Debug.LogWarning($"Couldn't find {data.originalLabel}.");
                    continue;
                }
                data.label = savedData.label;
                data.width = savedData.width;
                data.height = savedData.height;
                data.pivot = savedData.pivot;
                data.level = savedData.level;
                data.order = savedData.order;
                data.animation = savedData.animation;
                data.hasFlipX = savedData.hasFlipX;
                data.hasFlipY = savedData.hasFlipY;
            }
            return datas;
        }

        private SpriteButtonData GetSavedData(List<SpriteButtonData> savedDatas, string originalLabel)
        {
            foreach (SpriteButtonData data in savedDatas)
            {
                if (data.originalLabel == originalLabel)
                    return data;
            }
            return null;
        }

        private List<SpriteButtonData> GetSavedDatas(string savePath, string sourcePath)
        {
            List<IJsonObject> items = ReadSavePath(savePath);
            int index = GetIndexOfSourcePath(items, sourcePath);
            if (index == -1)
                return new();

            IJsonObject item = items[index];
            List<IJsonObject> content = item.GetArray("content");
            List<SpriteButtonData> result = new();
            foreach (IJsonObject sprite in content)
            {
                result.Add(new()
                {
                    label = sprite.GetString("label"),
                    width = sprite.GetInt("width"),
                    height = sprite.GetInt("height"),
                    pivot = new(sprite.GetFloat("pivot_x"), sprite.GetFloat("pivot_y")),
                    level = sprite.GetInt("level"),
                    order = sprite.GetInt("order"),
                    animation = sprite.GetString("animation"),
                    hasFlipX = sprite.GetBool("has_flip_x"),
                    hasFlipY = sprite.GetBool("has_flip_y"),
                    originalLabel = sprite.GetString("original_label"),
                });
            }
            return result;
        }

        /*
            [
                {source_path: "", content: [ {label, width ...} ]}
            ]
        */
        public List<IJsonObject> ReadSavePath(string savePath)
        {
            if (!File.Exists(savePath))
                return null;
            string jsonContent = File.ReadAllText(savePath);
            return _jsonSerializer.DeserializeToArray(jsonContent);
        }

        public int GetIndexOfSourcePath(List<IJsonObject> items, string sourcePath)
        {
            if (items == null)
                return -1;

            for (int i = 0; i < items.Count; i++)
            {
                IJsonObject item = items[i];
                if (sourcePath == item.GetString("source_path"))
                    return i;
            }
            return -1;
        }
    }
}