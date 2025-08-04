using System.Collections.Generic;
using System.IO;
using AddressablesTools;
using AddressablesTools.Catalog;
using AddressablesTools.Classes;
using AssetsTools.NET;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Writer.AddressableTools
{
    public class AddrTool
    {
        private static bool IsUnityFS(string path)
        {
            const string unityFs = "UnityFS";
            using AssetsFileReader reader = new(path);
            if (reader.BaseStream.Length < unityFs.Length)
            {
                return false;
            }

            return reader.ReadStringLength(unityFs.Length) == unityFs;
        }

        private static void PatchCrcRecursive(ResourceLocation thisRsrc, HashSet<ResourceLocation> seenRsrcs)
        {
            // I think this can't happen right now, resources are duplicated every time
            if (seenRsrcs.Contains(thisRsrc))
                return;

            var data = thisRsrc.Data;
            if (data is WrappedSerializedObject { Object: AssetBundleRequestOptions abro })
            {
                abro.Crc = 0;
            }

            seenRsrcs.Add(thisRsrc);
            foreach (var childRsrc in thisRsrc.Dependencies)
            {
                PatchCrcRecursive(childRsrc, seenRsrcs);
            }
        }

        public static void PatchCrc(string path)
        {
            bool fromBundle = IsUnityFS(path);

            ContentCatalogData ccd;
            CatalogFileType fileType = CatalogFileType.None;

            if (fromBundle)
            {
                ccd = AddressablesCatalogFileParser.FromBundle(path);
            }
            else
            {
                using (FileStream fs = File.OpenRead(path))
                {
                    fileType = AddressablesCatalogFileParser.GetCatalogFileType(fs);
                }

                switch (fileType)
                {
                    case CatalogFileType.Json:
                        ccd = AddressablesCatalogFileParser.FromJsonString(File.ReadAllText(path));
                        break;
                    case CatalogFileType.Binary:
                        ccd = AddressablesCatalogFileParser.FromBinaryData(File.ReadAllBytes(path));
                        break;
                    default:
                        Debug.LogError("Not a valid catalog file");
                        return;
                }
            }

            Debug.Log("Patching CRC...");

            var seenRsrcs = new HashSet<ResourceLocation>();
            foreach (var resourceList in ccd.Resources.Values)
            {
                foreach (var rsrc in resourceList)
                {
                    if (rsrc.Dependencies != null)
                    {
                        PatchCrcRecursive(rsrc, seenRsrcs);
                    }

                    if (rsrc.ProviderId == "UnityEngine.ResourceManagement.ResourceProviders.AssetBundleProvider")
                    {
                        if (rsrc.Data is WrappedSerializedObject { Object: AssetBundleRequestOptions abro })
                        {
                            abro.Crc = 0;
                        }
                    }
                }
            }

            string patchedPath = path + ".patched";

            if (fromBundle)
            {
                AddressablesCatalogFileParser.ToBundle(ccd, path, patchedPath);
            }
            else
            {
                switch (fileType)
                {
                    case CatalogFileType.Json:
                        File.WriteAllText(patchedPath, AddressablesCatalogFileParser.ToJsonString(ccd));
                        break;
                    case CatalogFileType.Binary:
                        File.WriteAllBytes(patchedPath, AddressablesCatalogFileParser.ToBinaryData(ccd));
                        break;
                }
            }

            string oldPath = path + ".old";
            string newPath = path;
            patchedPath = path + ".patched";

            // Backup original catalog
            if (File.Exists(oldPath))
                File.Delete(oldPath);
            File.Move(newPath, oldPath);

            // Overwrite original with patched
            if (File.Exists(newPath))
                File.Delete(newPath);
            File.Move(patchedPath, newPath);

            Debug.Log("CRC patching complete.");
        }
    }
}
