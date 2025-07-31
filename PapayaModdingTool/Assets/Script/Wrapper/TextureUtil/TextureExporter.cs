using System.Diagnostics;
using System.IO;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using AssetsTools.NET.Texture;
using PapayaModdingTool.Assets.Script.Misc.AppCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PapayaModdingTool.Assets.Script.Wrapper.TextureUtil
{
    public class TextureExporter
    {
        private readonly AssetsManager _assetsManager;
        private readonly TextureImportExport _textureImportExport;

        public TextureExporter(AppEnvironment appEnvironment)
        {
            _assetsManager = appEnvironment.AssetsManager;
            _textureImportExport = new(appEnvironment.Wrapper.TextureEncoderDecoder,
                                        appEnvironment.Wrapper.TextureEncoderDecoder);
        }

        public void ExportTextureWithPathIdTo(string savePath,
                                                AssetsFileInstance assetInst,
                                                long pathID)
        {
            AssetFileInfo info = assetInst.file.GetAssetInfo(pathID);
            TextureHelper textureHelper = new(_assetsManager);
            AssetTypeValueField texBase = textureHelper.GetByteArrayTexture(assetInst, info);

            uint platform = assetInst.file.Metadata.TargetPlatform;
            byte[] platformBlob = textureHelper.GetPlatformBlob(texBase);

            TextureFile texFile = TextureFile.ReadTextureFile(texBase);

            if (!textureHelper.GetResSTexture(texFile, assetInst))
            {
                UnityEngine.Debug.LogError("Texture uses resS, but the resS file wasn't found");
                return;
            }

            byte[] data = textureHelper.GetRawTextureBytes(texFile, assetInst);
            Image<Rgba32> imageToExport = _textureImportExport.Export(data,
                                        texFile.m_Width,
                                        texFile.m_Height,
                                        (TextureFormat)texFile.m_TextureFormat,
                                        platform,
                                        platformBlob);

            if (!Directory.Exists(savePath))
                Directory.CreateDirectory(savePath);

            TextureImportExport.SaveImageAtPath(imageToExport, Path.Combine(savePath, texFile.m_Name+".png"));
        }
    }
}