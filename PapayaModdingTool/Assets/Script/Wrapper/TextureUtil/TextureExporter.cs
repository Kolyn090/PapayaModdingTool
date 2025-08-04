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
        private readonly TextureHelper _textureHelper;

        public TextureExporter(AppEnvironment appEnvironment)
        {
            _assetsManager = appEnvironment.AssetsManager;
            _textureImportExport = new(appEnvironment.Wrapper.TextureEncoderDecoder,
                                        appEnvironment.Wrapper.TextureEncoderDecoder);
            _textureHelper = new(_assetsManager);
        }

        public void ExportTextureWithPathIdTo(string savePath,
                                                AssetsFileInstance assetInst,
                                                long pathID)
        {
            AssetFileInfo info = assetInst.file.GetAssetInfo(pathID);
            AssetTypeValueField texBase = _textureHelper.GetByteArrayTexture(assetInst, info);
            ExportTextureWithPathIdTo(savePath, assetInst, texBase);
        }


        public string ExportTextureWithPathIdTo(string savePath,
                                                AssetsFileInstance assetInst,
                                                AssetTypeValueField texBase,
                                                bool includePathIDInName=false,
                                                long pathID=0)
        {
            uint platform = assetInst.file.Metadata.TargetPlatform;
            byte[] platformBlob = _textureHelper.GetPlatformBlob(texBase);

            TextureFile texFile = TextureFile.ReadTextureFile(texBase);

            if (!_textureHelper.GetResSTexture(texFile, assetInst))
            {
                UnityEngine.Debug.LogError("Texture uses resS, but the resS file wasn't found");
                return "";
            }

            byte[] data = _textureHelper.GetRawTextureBytes(texFile, assetInst);
            Image<Rgba32> imageToExport = _textureImportExport.Export(data,
                                        texFile.m_Width,
                                        texFile.m_Height,
                                        (TextureFormat)texFile.m_TextureFormat,
                                        platform,
                                        platformBlob);

            if (!Directory.Exists(savePath))
                Directory.CreateDirectory(savePath);

            string suffix = includePathIDInName ? $"_{pathID}" : "";
            string fileName = $"{texFile.m_Name}{suffix}.png";
            string fullPath = Path.Combine(savePath, fileName);
            TextureImportExport.SaveImageAtPath(imageToExport, fullPath);
            return fullPath;
        }
    }
}
