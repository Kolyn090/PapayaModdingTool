using System.Collections.Generic;
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

        public TextureAssetsLoader(AppEnvironment appEnvironment)
        {
            _assetsManager = appEnvironment.AssetsManager;
            _bundleReader = new(appEnvironment.AssetsManager, appEnvironment.Dispatcher);
            _textureExporter = new(appEnvironment);
            _dumpImportExport = new(appEnvironment.AssetsManager);
        }

        public void LoadTextureAssets(LoadFileInfo loadInfo, string projectName, string textureSavePath)
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
                    _textureExporter.ExportTextureWithPathIdTo(textureSavePath, assetsInst, texBase);
                }
                return;
            }

            AssetTypeValueField onlyTexBase = _assetsManager.GetBaseField(assetsInst, texInfos[0]);
            string textureFullPath = _textureExporter.ExportTextureWithPathIdTo(textureSavePath, assetsInst, onlyTexBase);

            // Read dumps and save them as Source Dumps
            // !!! Don't think Atlas for now
            // !!! Assume it's one Texture2D + many Sprites
            // 1. Create Source Dump folder
            string projectPath = Path.Combine(PredefinedPaths.ProjectsPath, projectName);
            string texturePath = Path.Combine(projectPath, "Texture");
            string fileFolderPath = Path.Combine(texturePath, loadInfo.folder);
            string sourceDumpsPath = Path.Combine(fileFolderPath, "Source Dump");
            if (!Directory.Exists(sourceDumpsPath))
                Directory.CreateDirectory(sourceDumpsPath);
            // 2. Look for and export all Sprite Dumps
            List<AssetFileInfo> spriteInfos = assetsInst.file.GetAssetsOfType(AssetClassID.Sprite);
            List<AssetTypeValueField> spriteFields = new();
            foreach (var spriteInfo in spriteInfos)
            {
                AssetTypeValueField baseField = _assetsManager.GetBaseField(assetsInst, spriteInfo);
                string dumpFileName = baseField["m_Name"].AsString + "-" +
                                        cabCode + "-" +
                                        spriteInfo.PathId.ToString() + ".json";
                string writePath = Path.Combine(sourceDumpsPath, dumpFileName);
                AssetTypeValueField spriteField = _dumpImportExport.SingleExportJsonDumpInBundle(assetsInst, spriteInfo.PathId, writePath);
                spriteFields.Add(spriteField);
            }

            AssetDatabase.ImportAsset(textureFullPath, ImportAssetOptions.ForceSynchronousImport);
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(textureFullPath);

            // Use dumps to slice the texture
            SpritesheetFromDump spritesheetFromDump = new(texture, sourceDumpsPath, gamePPU: 100);
            spritesheetFromDump.Import();
        }
    }
}