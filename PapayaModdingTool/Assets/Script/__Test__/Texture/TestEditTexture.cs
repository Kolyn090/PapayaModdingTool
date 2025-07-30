using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using AssetsTools.NET.Texture;
using PapayaModdingTool.Assets.Script.__Test__.TestUtil;
using PapayaModdingTool.Assets.Script.Misc.AppCore;
using PapayaModdingTool.Assets.Script.Reader;
using PapayaModdingTool.Assets.Script.Wrapper.TextureUtil;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using UnityEngine;

using TextureFormat = AssetsTools.NET.Texture.TextureFormat;

namespace PapayaModdingTool.Assets.Script.__Test__.Texture
{
    public class TestEditTexture : MonoBehaviour, ITestable
    {
        public void Test(Action onComplete)
        {
            TestHelper.TestOnCleanLabDesk(() =>
            {
                AppEnvironment appEnvironment = new();
                BundleReader bundleReader = new(appEnvironment.AssetsManager, appEnvironment.Dispatcher);
                string DATA_PATH = Path.Combine(PredefinedTestPaths.LabDeskPath, "GameData");
                string bundlePath = Path.Combine(DATA_PATH, "spriteassetgroup_assets_assets/needdynamicloadresources/spritereference/unit_hero_gangdan.asset_266134690b1c6daffbecb67815ff8868.bundle");

                (BundleFileInstance bunInst, AssetsFileInstance assetInst) = bundleReader.ReadBundle(bundlePath);
                long texturePathID = -5051031092977008815;
                string ASSET_PATH = Path.Combine(PredefinedTestPaths.LabDeskPath, "Asset");
                string texture4ReplacePath = Path.Combine(ASSET_PATH, "spriteassetgroup_assets_assets/needdynamicloadresources/spritereference/unit_hero_gangdan-CAB-7570f5ae7807c50c425af095d0113220--5051031092977008815.png");
                ReplaceTexture(appEnvironment.AssetsManager,
                                appEnvironment.Wrapper.TextureImportExport,
                                assetInst,
                                bunInst,
                                texturePathID,
                                texture4ReplacePath);
                onComplete?.Invoke();
            });
        }

        private void ReplaceTexture(AssetsManager am,
                                    ITextureImporter<Rgba32> textureImporter,
                                    AssetsFileInstance assetInst,
                                    BundleFileInstance bunInst,
                                    long pathID,
                                    string textureImagePath)
        {
            // EditTextureOption.cs - ExecutePlugin
            AssetFileInfo info = assetInst.file.GetAssetInfo(pathID);
            TextureHelper textureHelper = new(am);

            AssetTypeValueField texBase = textureHelper.GetByteArrayTexture(assetInst, info);
            TextureFile texFile = TextureFile.ReadTextureFile(texBase);
            bool hasMipMaps = texFile.m_MipMap;
            bool readable = texFile.m_IsReadable;
            int filterMode = texFile.m_TextureSettings.m_FilterMode;
            string anisotFilter = texFile.m_TextureSettings.m_Aniso.ToString();
            string mipmapBias = texFile.m_TextureSettings.m_MipBias.ToString();
            int wrapmodeU = texFile.m_TextureSettings.m_WrapU;
            int wrapmodeV = texFile.m_TextureSettings.m_WrapV;
            string lightmapFormat = "0x" + texFile.m_LightmapFormat.ToString("X2");
            int colorSpace = texFile.m_ColorSpace;

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
            byte[] encImageBytes = textureImporter.Import(imgToImport, (TextureFormat)format, out int width, out int height, ref mips, platform, platformBlob);
            if (encImageBytes == null)
            {
                Debug.LogError($"Failed to encode texture format {(TextureFormat)format}!");
                return;
            }

            texBase["m_StreamData"]["offset"].AsInt = 0;
            // texBase["m_StreamData"]["size"].AsInt = 0;

            if (!texBase["m_MipMap"].IsDummy)
                texBase["m_MipMap"].AsBool = hasMipMaps;

            if (!texBase["m_MipCount"].IsDummy)
                texBase["m_MipCount"].AsInt = mips;

            if (!texBase["m_ReadAllowed"].IsDummy)
                texBase["m_ReadAllowed"].AsBool = readable;

            texBase["m_TextureSettings"]["m_FilterMode"].AsInt = filterMode;
            texBase["m_TextureSettings"]["m_Aniso"].AsInt = int.Parse(anisotFilter);
            texBase["m_TextureSettings"]["m_MipBias"].AsInt = int.Parse(mipmapBias);

            if (!texBase["m_TextureSettings"]["m_WrapU"].IsDummy)
                texBase["m_TextureSettings"]["m_WrapU"].AsInt = wrapmodeU;

            if (!texBase["m_TextureSettings"]["m_WrapV"].IsDummy)
                texBase["m_TextureSettings"]["m_WrapV"].AsInt = wrapmodeV;

            if (lightmapFormat.StartsWith("0x"))
            {
                if (int.TryParse(lightmapFormat, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out int lightFmt))
                    texBase["m_LightmapFormat"].AsInt = lightFmt;
            }
            else
            {
                if (int.TryParse(lightmapFormat, out int lightFmt))
                    texBase["m_LightmapFormat"].AsInt = lightFmt;
            }

            if (!texBase["m_ColorSpace"].IsDummy)
                texBase["m_ColorSpace"].AsInt = colorSpace;

            texBase["m_TextureFormat"].AsInt = format;

            if (!texBase["m_CompleteImageSize"].IsDummy)
                texBase["m_CompleteImageSize"].AsInt = encImageBytes.Length;

            texBase["m_StreamData"]["size"].AsInt = encImageBytes.Length;

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
            using (MemoryStream ms = new MemoryStream())
            using (AssetsFileWriter w = new AssetsFileWriter(ms))
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
            using (AssetsFileWriter w = new AssetsFileWriter(fs))
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

            am.UnloadAllBundleFiles();
        }
    }
}