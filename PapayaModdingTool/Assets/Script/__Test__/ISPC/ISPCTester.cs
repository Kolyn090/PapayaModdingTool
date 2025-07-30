using System;
using System.IO;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using PapayaModdingTool.Assets.Script.__Test__.TestUtil;
using PapayaModdingTool.Assets.Script.Misc.AppCore;
using PapayaModdingTool.Assets.Script.Reader;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using UABS.Assets.Script.__Test__;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.__Test__
{
    public class ISPCTester : MonoBehaviour, ITestable
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
                ConvertToBC7(appEnvironment, appEnvironment.AssetsManager, assetInst, bunInst, texturePathID, texture4ReplacePath, bundlePath);
                onComplete?.Invoke();
            });
        }

        private void ConvertToBC7(AppEnvironment appEnvironment,
                                    AssetsManager am,
                                    AssetsFileInstance assetInst,
                                    BundleFileInstance bunInst,
                                    long pathID,
                                    string textureImagePath,
                                    string bundlePath)
        {
            AssetFileInfo info = assetInst.file.GetAssetInfo(pathID);
            TextureHelper textureHelper = new(am);
            AssetTypeValueField texBase = textureHelper.GetByteArrayTexture(assetInst, info);
            uint platform = assetInst.file.Metadata.TargetPlatform;
            byte[] platformBlob = textureHelper.GetPlatformBlob(texBase);
            Image<Rgba32> imgToImport = Image.Load<Rgba32>(textureImagePath);
            int format = texBase["m_TextureFormat"].AsInt;
            int width = imgToImport.Width, height = imgToImport.Height;
            byte[] rgbaBytes = new byte[width * height * 4];
            imgToImport.CopyPixelDataTo(rgbaBytes);
            byte[] imageBytes = rgbaBytes;
            byte[] ispcBytes = ISPCWrapper.EncodeISPC(imageBytes, width, height, (AssetsTools.NET.Texture.TextureFormat)TextureFormat.BC7);
            Debug.Log($"ISPC Tester: The encoded size is {ispcBytes.Length}");
        }
    }
}