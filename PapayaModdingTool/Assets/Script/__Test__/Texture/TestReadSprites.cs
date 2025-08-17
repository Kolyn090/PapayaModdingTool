using System;
using System.Collections.Generic;
using System.IO;
using AssetsTools.NET.Extra;
using PapayaModdingTool.Assets.Script.__Test__.TestUtil;
using PapayaModdingTool.Assets.Script.Misc.AppCore;
using PapayaModdingTool.Assets.Script.Reader;
using PapayaModdingTool.Assets.Script.Reader.ImageDecoder;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.__Test__.Texture
{
    public class TestReadSprites : MonoBehaviour, ITestable
    {
        // No Atlas
        public void Test(Action onComplete)
        {
            TestHelper.TestOnCleanLabDesk(() =>
            {
                AppEnvironment appEnvironment = new();
                BundleReader bundleReader = new(appEnvironment.AssetsManager, appEnvironment.Dispatcher);
                string DATA_PATH = Path.Combine(PredefinedTestPaths.LabDeskPath, "GameData");
                string bundlePath = Path.Combine(DATA_PATH, "spriteassetgroup_assets_assets/needdynamicloadresources/spritereference/unit_hero_gangdan.asset_266134690b1c6daffbecb67815ff8868.bundle");
                (BundleFileInstance bunInst, AssetsFileInstance assetsInst) = bundleReader.ReadBundle(bundlePath);

                List<Texture2D> bytes = ImageReader.ReadSprites(assetsInst, appEnvironment.AssetsManager, appEnvironment.Wrapper.TextureEncoderDecoder);
                string ASSET_EXPORT_PATH = Path.Combine(PredefinedTestPaths.LabDeskPath, "Assets_Export");
                string exportImageDestination = Path.Combine(ASSET_EXPORT_PATH, "spriteassetgroup_assets_assets/needdynamicloadresources/spritereference");

                SaveImages(bytes, exportImageDestination);
                appEnvironment.AssetsManager.UnloadAll();
                onComplete?.Invoke();
            });
        }

        private static void SaveImages(List<Texture2D> images, string dir)
        {
            // Ensure the directory exists
            Directory.CreateDirectory(dir);

            for (int i = 0; i < images.Count; i++)
            {
                Texture2D tex = images[i];

                // Encode Texture2D to PNG bytes
                byte[] pngBytes = tex.EncodeToPNG();

                string filePath = Path.Combine(dir, $"image_{i}.png");
                File.WriteAllBytes(filePath, pngBytes);

                Debug.Log($"Exported {filePath}, PNG Length: {pngBytes.Length}");
            }
        }
    }
}