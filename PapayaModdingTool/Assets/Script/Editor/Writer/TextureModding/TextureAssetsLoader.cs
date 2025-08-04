using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using PapayaModdingTool.Assets.Script.DataStruct.FileRead;
using PapayaModdingTool.Assets.Script.Misc.AppCore;
using PapayaModdingTool.Assets.Script.Misc.Paths;
using PapayaModdingTool.Assets.Script.Reader;
using PapayaModdingTool.Assets.Script.Wrapper.TextureUtil;
using PapayaModdingTool.Assets.Script.Writer;
using UnityEditor;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.Writer.TextureModding
{
    public class TextureAssetsLoader
    {
        private readonly AssetsManager _assetsManager;
        private readonly BundleReader _bundleReader;
        private readonly TextureExporter _textureExporter;
        private readonly DumpImportExport _dumpImportExport;
        private readonly Texture2dReplacer _texture2dReplacer;

        public TextureAssetsLoader(AppEnvironment appEnvironment)
        {
            _assetsManager = appEnvironment.AssetsManager;
            _bundleReader = new(appEnvironment.AssetsManager, appEnvironment.Dispatcher);
            _textureExporter = new(appEnvironment);
            _dumpImportExport = new(appEnvironment.AssetsManager);
            _texture2dReplacer = new(appEnvironment.AssetsManager, appEnvironment.Wrapper.TextureImportExport);
        }

        public void LoadTextureAssets(LoadFileInfo loadInfo, string projectName, string textureSavePath, int gamePPU)
        {
            // Load the texture from bundle and save it to the correct place
            // 1. Read the bundle, check if valid
            string completePath = loadInfo.absolute_path;
            (BundleFileInstance bunInst, AssetsFileInstance assetsInst) = _bundleReader.ReadBundle(completePath);
            if (bunInst == null && assetsInst == null)
            {
                UnityEngine.Debug.LogWarning($"{completePath} is not a valid file. Abort.");
                return;
            }
            string cabCode = DumpReader.ReadCABCode(completePath);

            // 2. Read all Texture2D assets from it
            List<AssetFileInfo> texInfos = assetsInst.file.GetAssetsOfType(AssetClassID.Texture2D);
            if (texInfos.Count == 0)
            {
                UnityEngine.Debug.LogWarning($"{completePath} has no Texture2D asset. Abort.");
                return;
            }

            if (texInfos.Count > 1)
            {
                // There is more than one Texture found in the file, just export the textures and stop
                foreach (var texInfo in texInfos)
                {
                    AssetTypeValueField texBase = _assetsManager.GetBaseField(assetsInst, texInfo);
                    _textureExporter.ExportTextureWithPathIdTo(textureSavePath, assetsInst, texBase, true, texInfo.PathId);
                }
                return;
            }

            AssetTypeValueField onlyTexBase = _assetsManager.GetBaseField(assetsInst, texInfos[0]);
            string textureFullPath = _textureExporter.ExportTextureWithPathIdTo(textureSavePath, assetsInst, onlyTexBase, true, texInfos[0].PathId);

            // Read dumps and save them as Source Dumps
            // !!! Don't think Atlas for now
            // !!! Assume it's one Texture2D + many Sprites
            // 1. Create Source Dump folder
            string projectPath = Path.Combine(PredefinedPaths.ProjectsPath, projectName);
            string fileFolderPath = Path.Combine(projectPath, loadInfo.folder);
            string texturePath = Path.Combine(fileFolderPath, "Texture");
            string sourceDumpsPath = Path.Combine(texturePath, "Source Dump");
            if (!Directory.Exists(sourceDumpsPath))
                Directory.CreateDirectory(sourceDumpsPath);
            // 2. Look for and export all Sprite Dumps
            List<AssetFileInfo> spriteInfos = assetsInst.file.GetAssetsOfType(AssetClassID.Sprite);
            // List<AssetTypeValueField> spriteFields = new();
            foreach (var spriteInfo in spriteInfos)
            {
                AssetTypeValueField baseField = _assetsManager.GetBaseField(assetsInst, spriteInfo);
                string dumpFileName = baseField["m_Name"].AsString + "-" +
                                        cabCode + "-" +
                                        spriteInfo.PathId.ToString() + ".json";
                string writePath = Path.Combine(sourceDumpsPath, dumpFileName);
                AssetTypeValueField spriteField = _dumpImportExport.SingleExportJsonDumpInBundle(assetsInst, spriteInfo.PathId, writePath);
                // spriteFields.Add(spriteField);
            }

            AssetDatabase.ImportAsset(textureFullPath, ImportAssetOptions.ForceSynchronousImport);
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(textureFullPath);

            // Use dumps to slice the texture
            SpritesheetFromDump spritesheetFromDump = new(texture, sourceDumpsPath, gamePPU: gamePPU);
            spritesheetFromDump.Import();
        }

        public void LoadTextureOnly(string bundlePath, string textureSavePath)
        {
            // Load the texture from bundle and save it to the correct place
            // 1. Read the bundle, check if valid
            string completePath = bundlePath;
            (BundleFileInstance bunInst, AssetsFileInstance assetsInst) = _bundleReader.ReadBundle(completePath);
            if (bunInst == null && assetsInst == null)
            {
                UnityEngine.Debug.LogWarning($"{completePath} is not a valid file. Abort.");
                return;
            }

            // 2. Read all Texture2D assets from it
            List<AssetFileInfo> texInfos = assetsInst.file.GetAssetsOfType(AssetClassID.Texture2D);
            if (texInfos.Count == 0)
            {
                UnityEngine.Debug.LogWarning($"{completePath} has no Texture2D asset. Abort.");
                return;
            }

            if (texInfos.Count > 1)
            {
                // There is more than one Texture found in the file, just export the textures and stop
                foreach (var texInfo in texInfos)
                {
                    AssetTypeValueField texBase = _assetsManager.GetBaseField(assetsInst, texInfo);
                    _textureExporter.ExportTextureWithPathIdTo(textureSavePath, assetsInst, texBase);
                }
                return;
            }

            AssetTypeValueField onlyTexBase = _assetsManager.GetBaseField(assetsInst, texInfos[0]);
            _textureExporter.ExportTextureWithPathIdTo(textureSavePath, assetsInst, onlyTexBase);
        }

        public void ExportSpriteDumpsOnly(string bundlePath, string fileFolder, string projectName)
        {
            string cabCode = DumpReader.ReadCABCode(bundlePath);
            (BundleFileInstance _, AssetsFileInstance assetsInst) = _bundleReader.ReadBundle(bundlePath);

            // Read dumps and save them as Source Dumps
            // !!! Don't think Atlas for now
            // !!! Assume it's one Texture2D + many Sprites
            // 1. Create Source Dump folder
            string projectPath = Path.Combine(PredefinedPaths.ProjectsPath, projectName);
            string fileFolderPath = Path.Combine(projectPath, fileFolder);
            string texturePath = Path.Combine(fileFolderPath, "Texture");
            string sourceDumpsPath = Path.Combine(texturePath, "Owning Dump");
            if (!Directory.Exists(sourceDumpsPath))
                Directory.CreateDirectory(sourceDumpsPath);
            // 2. Look for and export all Sprite Dumps
            List<AssetFileInfo> spriteInfos = assetsInst.file.GetAssetsOfType(AssetClassID.Sprite);
            // List<AssetTypeValueField> spriteFields = new();
            foreach (var spriteInfo in spriteInfos)
            {
                AssetTypeValueField baseField = _assetsManager.GetBaseField(assetsInst, spriteInfo);
                string dumpFileName = baseField["m_Name"].AsString + "-" +
                                        cabCode + "-" +
                                        spriteInfo.PathId.ToString() + ".json";
                string writePath = Path.Combine(sourceDumpsPath, dumpFileName);
                _dumpImportExport.SingleExportJsonDumpInBundle(assetsInst, spriteInfo.PathId, writePath);
                // spriteFields.Add(spriteField);
            }
        }

        public void ImportTexture(string bundlePath, string importTexturePath)
        {
            // The names of the texture are unchanged after everything
            // Just import by matching name 

            // There could be multiple textures in the given texture path
            // Import them all

            // Assume files the all images
            string[] allImages = Directory.GetFiles(importTexturePath, "*", SearchOption.TopDirectoryOnly);
            foreach (string image in allImages)
            {
                (BundleFileInstance bunInst, AssetsFileInstance assetsInst) = _bundleReader.ReadBundle(bundlePath);
                // !!! The image must have path id in its last regex underscore
                // Extract the path id
                string imageName = Path.GetFileNameWithoutExtension(image);
                (string, string) splitByLastUnderscore = PathUtils.SplitByLastRegex(imageName, "_");
                (string _, string pathIDStr) = splitByLastUnderscore;
                long pathID = long.Parse(pathIDStr.Replace("_", ""));

                _texture2dReplacer.ReplaceTextureInBundle(assetsInst, bunInst, pathID, image);
                Debug.Log($"Replaced texture {imageName}");
            }
        }
    }
}
