using AssetsTools.NET;
using AssetsTools.NET.Extra;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.DataStruct.TextureData
{
    public class Texture2DButtonData
    {
        public string label;
        public string sourcePath;
        public string fileFolderName;

        // Style 1: Read from bundle
        public Texture2D texture;
        public AssetsFileInstance assetsInst;
        public AssetFileInfo assetInfo;

        // Style 2: Provide dir to imported Textures
        public string importedTexturesPath;

        public bool IsStyle1 => texture != null && assetsInst != null && assetInfo != null;
        public bool IsStyle2 => !IsStyle1 && !string.IsNullOrWhiteSpace(importedTexturesPath);
    }
}