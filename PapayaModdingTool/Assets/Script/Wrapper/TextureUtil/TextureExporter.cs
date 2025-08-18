using System.IO;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using AssetsTools.NET.Texture;
using PapayaModdingTool.Assets.Script.Misc.AppCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using UnityEditor;
using UnityEngine;

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
                                                AssetsFileInstance assetsInst,
                                                long pathID)
        {
            AssetFileInfo info = assetsInst.file.GetAssetInfo(pathID);
            AssetTypeValueField texBase = _textureHelper.GetByteArrayTexture(assetsInst, info);
            ExportTextureWithPathIdTo(savePath, assetsInst, texBase);
        }

        public string ExportTextureWithPathIdTo(string savePath,
                                                AssetsFileInstance assetsInst,
                                                AssetTypeValueField texBase,
                                                bool includePathIDInName = false,
                                                long pathID = 0)
        {
            uint platform = assetsInst.file.Metadata.TargetPlatform;
            byte[] platformBlob = _textureHelper.GetPlatformBlob(texBase);
            TextureFile texFile = TextureFile.ReadTextureFile(texBase);
            Image<Rgba32> imageToExport = GetImageToExport(platform, platformBlob, texFile, assetsInst);

            if (!Directory.Exists(savePath))
                Directory.CreateDirectory(savePath);

            // ? Name[_PathId].png
            string suffix = includePathIDInName ? $"_{pathID}" : "";
            string fileName = $"{texFile.m_Name}{suffix}.png";
            string fullPath = Path.Combine(savePath, fileName);
            TextureImportExport.SaveImageAtPath(imageToExport, fullPath);
            return fullPath;
        }

        public Texture2D ExportTextureWithPathIdAsTexture2D(AssetsFileInstance assetsInst,
                                                            AssetFileInfo texInfo)
        {
            AssetTypeValueField texBase = _assetsManager.GetBaseField(assetsInst, texInfo);
            return ExportTextureWithPathIdAsTexture2D(assetsInst, texBase);
        }

        public Texture2D ExportTextureWithPathIdAsTexture2D(AssetsFileInstance assetsInst,
                                                            AssetTypeValueField texBase)
        {
            uint platform = assetsInst.file.Metadata.TargetPlatform;
            byte[] platformBlob = _textureHelper.GetPlatformBlob(texBase);
            TextureFile texFile = TextureFile.ReadTextureFile(texBase);
            Image<Rgba32> imageToExport = GetImageToExport(platform, platformBlob, texFile, assetsInst);
            return ToTexture2D(imageToExport);
        }

        private Image<Rgba32> GetImageToExport(uint platform,
                                                byte[] platformBlob,
                                                TextureFile texFile,
                                                AssetsFileInstance assetsInst)
        {
            if (!_textureHelper.GetResSTexture(texFile, assetsInst))
            {
                UnityEngine.Debug.LogError("Texture uses resS, but the resS file wasn't found");
                return null;
            }

            byte[] data = _textureHelper.GetRawTextureBytes(texFile, assetsInst);
            Image<Rgba32> imageToExport = _textureImportExport.Export(data,
                                        texFile.m_Width,
                                        texFile.m_Height,
                                        (AssetsTools.NET.Texture.TextureFormat)texFile.m_TextureFormat,
                                        platform,
                                        platformBlob);
            return imageToExport;
        }

        private static Texture2D ToTexture2D(Image<Rgba32> image)
        {
            int width = image.Width;
            int height = image.Height;
            Texture2D texture = new(width, height, UnityEngine.TextureFormat.RGBA32, false);
            byte[] rgbaBytes = ToRgbaBytes(image);
            texture.LoadRawTextureData(rgbaBytes);
            texture.Apply();
            return texture;
        }

        private static byte[] ToRgbaBytes(Image<Rgba32> image)
        {
            int width = image.Width;
            int height = image.Height;
            int bytesPerPixel = 4; // R, G, B, A

            byte[] result = new byte[width * height * bytesPerPixel];

            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < height; y++)
                {
                    var row = accessor.GetRowSpan(y);
                    for (int x = 0; x < width; x++)
                    {
                        int idx = (y * width + x) * bytesPerPixel;
                        Rgba32 pixel = row[x];
                        result[idx + 0] = pixel.R;
                        result[idx + 1] = pixel.G;
                        result[idx + 2] = pixel.B;
                        result[idx + 3] = pixel.A;
                    }
                }
            });

            return result;
        }
    }
}
