using System;
using System.IO;
using AssetsTools.NET.Extra;
using PapayaModdingTool.Assets.Script.__Test__.TestUtil;
using PapayaModdingTool.Assets.Script.Misc.AppCore;
using PapayaModdingTool.Assets.Script.Reader;
using PapayaModdingTool.Assets.Script.Wrapper.TextureUtil;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.__Test__.Texture
{
    public class TestEditTextureStreamData : MonoBehaviour, ITestable
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
                Texture2dReplacer texture2DReplacer = new(appEnvironment.AssetsManager, appEnvironment.Wrapper.TextureImportExport);
                texture2DReplacer.ReplaceTextureInBundle(assetInst,
                                                bunInst,
                                                texturePathID,
                                                texture4ReplacePath);
                
                appEnvironment.AssetsManager.UnloadAll();
                onComplete?.Invoke();
            });
        }
    }
}