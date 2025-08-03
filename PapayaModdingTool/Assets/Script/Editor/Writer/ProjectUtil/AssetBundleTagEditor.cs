using UnityEditor;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.Writer.ProjectUtil
{
    public class AssetBundleTagEditor
    {
        public static void SetAssetBundleTag(string assetPath, string bundleName)
        {
            // Clear all other assets that already have this bundle name, except this one
            string[] allAssets = AssetDatabase.GetAllAssetPaths();
            foreach (var path in allAssets)
            {
                if (path == assetPath) continue; // skip current asset

                var otherImporter = AssetImporter.GetAtPath(path);
                if (otherImporter != null && otherImporter.assetBundleName == bundleName)
                {
                    otherImporter.assetBundleName = null;
                    otherImporter.SaveAndReimport();
                    Debug.Log($"Cleared conflicting bundle tag from: {path}");
                }
            }

            // Now assign to current asset
            AssetImporter importer = AssetImporter.GetAtPath(assetPath);
            if (importer != null)
            {
                importer.assetBundleName = bundleName;
                importer.SaveAndReimport();
                Debug.Log($"Set AssetBundle tag '{bundleName}' on: {assetPath}");
            }
            else
            {
                Debug.LogWarning("AssetImporter not found for: " + assetPath);
            }
        }
    }
}
