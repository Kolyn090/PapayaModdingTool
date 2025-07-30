using System.Collections.Generic;
using System.IO;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using UnityEngine;

using TextureFormat = AssetsTools.NET.Texture.TextureFormat;

namespace PapayaModdingTool.Assets.Script.Wrapper.TextureUtil
{
    public class Texture2dReplacer
    {
        private readonly AssetsManager _assetsManager;
        private readonly ITextureImporter<Rgba32> _textureImporter;

        public Texture2dReplacer(AssetsManager assetsManager, ITextureImporter<Rgba32> textureImporter)
        {
            _assetsManager = assetsManager;
            _textureImporter = textureImporter;
        }

        public void ReplaceTextureInBundle(AssetsFileInstance assetInst,
                                    BundleFileInstance bunInst,
                                    long pathID,
                                    string textureImagePath)
        {
            // EditTextureOption.cs - ExecutePlugin
            AssetFileInfo info = assetInst.file.GetAssetInfo(pathID);
            TextureHelper textureHelper = new(_assetsManager);
            AssetTypeValueField texBase = textureHelper.GetByteArrayTexture(assetInst, info);

            // EditDialog.axaml.cs - BtnSave_Click
            uint platform = assetInst.file.Metadata.TargetPlatform;
            byte[] platformBlob = textureHelper.GetPlatformBlob(texBase);
            Image<Rgba32> imgToImport = Image.Load<Rgba32>(textureImagePath);
            int format = texBase["m_TextureFormat"].AsInt;

            int mips = 1;
            if (!texBase["m_MipCount"].IsDummy)
                mips = texBase["m_MipCount"].AsInt;
            else if (TextureHelper.IsPo2(imgToImport.Width) && TextureHelper.IsPo2(imgToImport.Height))
                mips = textureHelper.GetMaxMipCount(imgToImport.Width, imgToImport.Height);
            byte[] encImageBytes = _textureImporter.Import(imgToImport, (TextureFormat)format, out int width, out int height, ref mips, platform, platformBlob);
            if (encImageBytes == null)
            {
                Debug.LogError($"Failed to encode texture format {(TextureFormat)format}!");
                return;
            }

            if (!texBase["m_CompleteImageSize"].IsDummy)
                texBase["m_CompleteImageSize"].AsInt = encImageBytes.Length;

            texBase["m_Width"].AsInt = width;
            texBase["m_Height"].AsInt = height;

            texBase["m_StreamData"]["path"].AsString = "";
            texBase["m_StreamData"]["offset"].AsInt = 0;
            texBase["m_StreamData"]["size"].AsInt = 0;
            AssetTypeValueField image_data = texBase["image data"];
            image_data.Value.ValueType = AssetValueType.ByteArray;
            image_data.TemplateField.ValueType = AssetValueType.ByteArray;
            image_data.AsByteArray = encImageBytes;

            // 1. Write patched asset to byte array
            byte[] savedAsset = texBase.WriteToByteArray();
            AssetsReplacerFromMemory replacer = new(
                info.PathId, info.TypeId, assetInst.file.GetScriptIndex(info), savedAsset);

            // 2. Write the modified assets file to a new memory stream
            byte[] modifiedAssetsFileBytes;
            using (MemoryStream ms = new())
            using (AssetsFileWriter w = new(ms))
            {
                assetInst.file.Write(w, 0, new List<AssetsReplacer> { replacer });
                modifiedAssetsFileBytes = ms.ToArray();
            }

            // 3. Create a bundle replacer with the updated assets file
            string assetsFileNameInBundle = assetInst.name; // e.g. "cab-xxxx"
            BundleReplacer bunReplacer = new BundleReplacerFromMemory(
                assetsFileNameInBundle, // original name inside bundle
                null,                   // don't rename
                true,                   // has serialized data
                modifiedAssetsFileBytes,
                0,                      // offset
                modifiedAssetsFileBytes.Length
            );

            // 4. Write the new bundle to disk
            string newName = "~" + bunInst.name;
            string dir = Path.GetDirectoryName(bunInst.path)!;
            string filePath = Path.Combine(dir, newName);
            string origFilePath = bunInst.path;

            using (FileStream fs = File.Open(filePath, FileMode.Create))
            using (AssetsFileWriter w = new(fs))
            {
                bunInst.file.Write(w, new List<BundleReplacer> { bunReplacer });
            }

            // 5. Overwrite original bundle file
            bunInst.file.Reader.Close();
            File.Delete(origFilePath);
            File.Move(filePath, origFilePath);

            // 6. Reload the new bundle
            bunInst.file = new AssetBundleFile();
            bunInst.file.Read(new AssetsFileReader(File.OpenRead(origFilePath)));

            _assetsManager.UnloadBundleFile(bunInst.path);
            _assetsManager.UnloadAssetsFile(assetInst);
        }
    }
}