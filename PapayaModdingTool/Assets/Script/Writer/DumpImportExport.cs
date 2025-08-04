using System.Collections.Generic;
using System.IO;
using System.Linq;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Writer
{
    // JSON only
    public class DumpImportExport
    {
        private readonly AssetsManager _assetsManager;

        public DumpImportExport(AssetsManager assetsManager)
        {
            _assetsManager = assetsManager;
        }

        public AssetTypeValueField SingleExportJsonDumpInBundle(AssetsFileInstance assetsInst,
                                            long pathID,
                                            string writeTo)
        {
            AssetFileInfo info = assetsInst.file.GetAssetInfo(pathID);
            AssetTypeValueField baseField = _assetsManager.GetBaseField(assetsInst, info);
            AssetImportExport exporter = new();

            using (FileStream fs = File.Open(writeTo, FileMode.Create))
            using (StreamWriter sw = new(fs))
            {
                exporter.DumpJsonAsset(sw, baseField);
            }

            return baseField;
        }

        public void SingleImportJsonDumpInBundle(long pathID,
                                            AssetsFileInstance assetsInst,
                                            string replaceFilePath,
                                            string bundlePath)
        {
            AssetFileInfo info = assetsInst.file.GetAssetInfo(pathID);

            using (FileStream fs = File.OpenRead(replaceFilePath))
            using (StreamReader sr = new(fs))
            {
                AssetImportExport importer = new();
                byte[] bytes = null;

                AssetTypeTemplateField tempField = _assetsManager.GetTemplateBaseField(assetsInst,
                                                                            assetsInst.file.Reader,
                                                                            info.AbsoluteByteStart,
                                                                            info.TypeId,
                                                                            assetsInst.file.GetScriptIndex(info),
                                                                            AssetReadFlags.None);
                bytes = importer.ImportJsonAsset(tempField, sr, out string exceptionMessage);

                if (bytes == null)
                {
                    Debug.LogError($"Something went wrong when reading the dump file: {exceptionMessage}");
                    return;
                }

                AssetsReplacer replacer = AssetImportExport.CreateAssetReplacer(info.PathId,
                                                                                info.TypeId,
                                                                                assetsInst.file.GetScriptIndex(info),
                                                                                bytes);

                AssetsFile file = assetsInst.file;
                List<AssetsReplacer> replacers = new() { replacer };

                string tempPath = bundlePath + ".tmp";

                using (FileStream outFs = File.Create(tempPath))
                using (AssetsFileWriter writer = new(outFs))
                {
                    file.Write(writer, 0, replacers, _assetsManager.ClassDatabase);
                }

                // Unload the file from AssetsManager before replacing
                _assetsManager.UnloadBundleFile(bundlePath);
                // _assetsManager.UnloadAssetsFile(assetsInst);

                File.Replace(tempPath, bundlePath, null); // replace original with modified
            }
        }

        public void BatchImportJsonDumpInBundle(BundleFileInstance bunInst,
                                                AssetsFileInstance assetsInst,
                                                List<(long, string)> items)
        {
            List<AssetsReplacer> replacers = new();
            foreach (var item in items)
            {
                (long pathID, string replaceFilePath) = item;
                AssetFileInfo info = assetsInst.file.GetAssetInfo(pathID);
                using (FileStream fs = File.OpenRead(replaceFilePath))
                using (StreamReader sr = new(fs))
                {
                    AssetImportExport importer = new();
                    byte[] bytes = null;
                    AssetTypeTemplateField tempField = _assetsManager.GetTemplateBaseField(assetsInst,
                                                                            assetsInst.file.Reader,
                                                                            info.AbsoluteByteStart,
                                                                            info.TypeId,
                                                                            assetsInst.file.GetScriptIndex(info),
                                                                            AssetReadFlags.None);
                    bytes = importer.ImportJsonAsset(tempField, sr, out string exceptionMessage);

                    if (bytes == null)
                    {
                        Debug.LogError($"Something went wrong when reading the dump file: {exceptionMessage}");
                        continue;
                    }

                    AssetsReplacer replacer = AssetImportExport.CreateAssetReplacer(info.PathId,
                                                                                    info.TypeId,
                                                                                    assetsInst.file.GetScriptIndex(info),
                                                                                    bytes);
                    replacers.Add(replacer);
                }
            }

            AssetsFile file = assetsInst.file;
            byte[] modifiedAssetsFileBytes;
            using (MemoryStream ms = new())
            using (AssetsFileWriter writer = new(ms))
            {
                file.Write(writer, 0, replacers);
                modifiedAssetsFileBytes = ms.ToArray();
            }

            string assetsFileNameInBundle = assetsInst.name;
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
            _assetsManager.UnloadAssetsFile(assetsInst);
        }
    }
}
