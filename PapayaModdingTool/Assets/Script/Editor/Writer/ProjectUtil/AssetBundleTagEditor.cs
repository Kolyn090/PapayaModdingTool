using UnityEditor;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.Writer.ProjectUtil
{
    public class AssetBundleTagEditor
    {
        public static void SetAssetBundleTag(string assetPath, string bundleName)
        {
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
