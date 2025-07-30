using System;
using System.Collections.Generic;
using System.IO;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using PapayaModdingTool.Assets.Script.__Test__.TestUtil;
using PapayaModdingTool.Assets.Script.Misc.AppCore;
using PapayaModdingTool.Assets.Script.Reader;
using PapayaModdingTool.Assets.Script.Wrapper.Json;
using PapayaModdingTool.Assets.Script.Writer;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.__Test__.AssetBundle
{
    public class TestExportDump : MonoBehaviour, ITestable
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
                long firstSpritePathID = -452988096852721839;

                string EXPORT_DUMP_PATH = Path.Combine(PredefinedTestPaths.LabDeskPath, "Dump_Export");
                string spritereferencePath = Path.Combine(EXPORT_DUMP_PATH, "spriteassetgroup_assets_assets/needdynamicloadresources/spritereference");
                Directory.CreateDirectory(spritereferencePath);
                string firstSpriteDumpPath = Path.Combine(spritereferencePath, "unit_hero_gangdan_0-CAB-7570f5ae7807c50c425af095d0113220--452988096852721839.json");

                DumpImportExport dumpImportExport = new(appEnvironment.AssetsManager);
                dumpImportExport.SingleExportJsonDumpInBundle(assetInst, firstSpritePathID, firstSpriteDumpPath);

                // To verify, look into the exported json file
                string exportJsonPath = Path.Combine(spritereferencePath, "unit_hero_gangdan_0-CAB-7570f5ae7807c50c425af095d0113220--452988096852721839.json");
                if (!File.Exists(exportJsonPath))
                {
                    Debug.Log("[✘] No json file exported.");
                }

                // Verify a few values
                string jsonContent = File.ReadAllText(exportJsonPath);
                IJsonObject jsonObject = appEnvironment.Wrapper.JsonSerializer.DeserializeToObject(jsonContent);

                bool pass = false;
                List<AssetFileInfo> assetInfos = assetInst.file.GetAssetsOfType(AssetClassID.Sprite);
                foreach (var assetInfo in assetInfos)
                {
                    if (assetInfo.PathId == firstSpritePathID)
                    {
                        AssetTypeValueField assetBase = appEnvironment.AssetsManager.GetBaseField(assetInst, assetInfo);
                        pass = assetBase["m_Name"].AsString == jsonObject.GetString("m_Name");
                        break;
                    }
                }

                if (pass)
                {
                    Debug.Log("[✔] Export Dump Test Succeed.");
                }
                else
                {
                    Debug.Log("[✘] Exported Json file doesn't match with the original.");
                }
                
                appEnvironment.AssetsManager.UnloadAll();
                onComplete?.Invoke();
            });
        }
    }
}