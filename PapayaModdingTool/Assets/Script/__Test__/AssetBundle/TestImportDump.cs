using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using PapayaModdingTool.Assets.Script.__Test__.TestUtil;
using PapayaModdingTool.Assets.Script.Misc.AppCore;
using PapayaModdingTool.Assets.Script.Reader;
using PapayaModdingTool.Assets.Script.Writer;
using PapayaModdingTool.Assets.Script.Wrapper.Json;

namespace PapayaModdingTool.Assets.Script.__Test__.AssetBundle
{
    public class TestImportDump : MonoBehaviour, ITestable
    {
        public void Test(Action onComplete)
        {
            TestHelper.TestOnCleanLabDesk(() =>
            {
                AppEnvironment appEnvironment = new();
                BundleReader bundleReader = new(appEnvironment.AssetsManager, appEnvironment.Dispatcher);
                string DATA_PATH = Path.Combine(PredefinedTestPaths.LabDeskPath, "GameData");
                string bundlePath = Path.Combine(DATA_PATH, "spriteassetgroup_assets_assets/needdynamicloadresources/spritereference/unit_hero_gangdan.asset_266134690b1c6daffbecb67815ff8868.bundle");

                (BundleFileInstance _, AssetsFileInstance assetInst) = bundleReader.ReadBundle(bundlePath);

                List<AssetFileInfo> assetInfos = assetInst.file.GetAssetsOfType(AssetClassID.Sprite);
                long firstSpritePathID = -452988096852721839;
                AssetTypeValueField firstAssetBase = null;
                foreach (var assetInfo in assetInfos)
                {
                    if (assetInfo.PathId == firstSpritePathID)
                    {
                        firstAssetBase = appEnvironment.AssetsManager.GetBaseField(assetInst, assetInfo);
                        break;
                    }
                }

                string DUMP_PATH = Path.Combine(PredefinedTestPaths.LabDeskPath, "Dump");
                string firstSpriteDumpPath = Path.Combine(DUMP_PATH, "spriteassetgroup_assets_assets/needdynamicloadresources/spritereference/unit_hero_gangdan_0-CAB-7570f5ae7807c50c425af095d0113220--452988096852721839.json");

                DumpImportExport dumpImportExport = new(appEnvironment.AssetsManager);
                dumpImportExport.SingleImportJsonDumpInBundle(firstSpritePathID, assetInst, firstSpriteDumpPath, bundlePath);

                // Verify the value has been successfully modified
                // Check if m_Rect.y are the same in import dump and modify asset
                string jsonContent = File.ReadAllText(firstSpriteDumpPath);
                IJsonObject jsonObject = appEnvironment.Wrapper.JsonSerializer.DeserializeToObject(jsonContent);

                if (firstAssetBase == null)
                {
                    Debug.Log($"[✘] Couldn't find the First Asset Base with path id {firstSpritePathID}.");
                }

                if (firstAssetBase["m_Rect"]["y"].AsFloat == jsonObject.GetObject("m_Rect").GetFloat("y"))
                {
                    Debug.Log("[✔] Import Dump Test Succeed.");
                }
                else
                {
                    Debug.Log("[✘] Modified asset doesn't match with the imported json dump.");
                }

                appEnvironment.AssetsManager.UnloadAll();
                onComplete?.Invoke();
            });
        }
    }
}