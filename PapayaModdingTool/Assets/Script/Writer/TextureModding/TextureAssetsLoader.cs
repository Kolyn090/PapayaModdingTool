using System.Collections.Generic;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using PapayaModdingTool.Assets.Script.DataStruct.FileRead;
using PapayaModdingTool.Assets.Script.Misc.AppCore;
using PapayaModdingTool.Assets.Script.Reader;
using PapayaModdingTool.Assets.Script.Wrapper.TextureUtil;

namespace PapayaModdingTool.Assets.Script.Writer.TextureModding
{
    public class TextureAssetsLoader
    {
        private readonly AssetsManager _assetsManager;
        private readonly BundleReader _bundleReader;
        private readonly TextureExporter _textureExporter;

        public TextureAssetsLoader(AppEnvironment appEnvironment)
        {
            _assetsManager = appEnvironment.AssetsManager;
            _bundleReader = new(appEnvironment.AssetsManager, appEnvironment.Dispatcher);
            _textureExporter = new(appEnvironment);
        }

        public void LoadTextureAssets(LoadFileInfo loadInfo, string savePath)
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

            // 2. Read all Texture2D assets from it
            List<AssetFileInfo> assetInfos = assetsInst.file.GetAssetsOfType(AssetClassID.Texture2D);
            if (assetInfos.Count == 0)
            {
                UnityEngine.Debug.LogWarning($"{completePath} has no Texture2D asset. Abort.");
                return;
            }

            if (assetInfos.Count > 1)
            {
                // There is more than one Texture found in the file, just export the textures and stop
                foreach (var assetInfo in assetInfos)
                {
                    AssetTypeValueField texBase = _assetsManager.GetBaseField(assetsInst, assetInfo);
                    _textureExporter.ExportTextureWithPathIdTo(savePath, assetsInst, texBase);
                }
                return;
            }

            // Read dumps and save them as Source Dumps

            // Use dumps to slice the texture
        }
    }
}