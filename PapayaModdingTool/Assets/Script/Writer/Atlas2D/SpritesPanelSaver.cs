using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using PapayaModdingTool.Assets.Script.DataStruct.TextureData;
using PapayaModdingTool.Assets.Script.Reader.Atlas2D;
using PapayaModdingTool.Assets.Script.Wrapper.Json;
using UnityEngine;

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

        // Called when sprite is duplicated. Copy all except label & original label & level & order!
        public void CopyAfterDuplication(string savePath,
                                                string sourcePath, // ! Warning: this has no connection with below
                                                string sourceOriginalLabel,
                                                string targetOriginalLabel)
        {
            List<SpriteButtonData> saveDatas = _reader.GetSavedDatas(savePath, sourcePath);
            if (saveDatas.Count == 0)
            {
                Debug.Log($"Failed to copy {sourceOriginalLabel} to {targetOriginalLabel} because save was not found.");
            }

            SpriteButtonData sourceSprite = saveDatas.FirstOrDefault(x => x.originalLabel == sourceOriginalLabel);
            if (sourceSprite != null)
            {
                SpriteButtonData targetSprite = saveDatas.FirstOrDefault(x => x.originalLabel == targetOriginalLabel);
                if (targetSprite != null)
                {
                    targetSprite.sprite = sourceSprite.sprite;
                    targetSprite.width = sourceSprite.width;
                    targetSprite.height = sourceSprite.height;
                    targetSprite.pivot = sourceSprite.pivot;
                    targetSprite.animation = sourceSprite.animation;
                    targetSprite.hasFlipX = sourceSprite.hasFlipX;
                    targetSprite.hasFlipY = sourceSprite.hasFlipY;
                }
                else
                {
                    saveDatas.Add(new()
                    {
                        label = targetOriginalLabel,
                        originalLabel = targetOriginalLabel,
                        sprite = sourceSprite.sprite,
                        width = sourceSprite.width,
                        height = sourceSprite.height,
                        pivot = sourceSprite.pivot,
                        animation = sourceSprite.animation,
                        hasFlipX = sourceSprite.hasFlipX,
                        hasFlipY = sourceSprite.hasFlipY
                    });
                }
            }
            else
            {
                Debug.Log($"Failed to find {sourceOriginalLabel}. Did you change its sprite's name?");
                return;
            }

            Save(savePath, sourcePath, saveDatas);
            Debug.Log($"Successfully copied {sourceOriginalLabel} to {targetOriginalLabel}.");
        }
    }
}