using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

namespace PapayaModdingTool.Assets.Script.Editor.Writer.ProjectUtil
{
    public class AssetBundleBuilder
    {
        public static void BuildAllAssetBundles(string bundleTag, string destination, BuildTarget target)
        {
            // Find all asset paths with the given bundle tag
            string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
            var assetsInBundle = allAssetPaths
                .Where(path => AssetImporter.GetAtPath(path)?.assetBundleName == bundleTag)
                .ToArray();

            if (assetsInBundle.Length == 0)
            {
                Debug.LogWarning("No assets found with bundle name: " + bundleTag);
                return;
            }

            // Define the build map using only the tagged assets
            AssetBundleBuild[] buildMap = new AssetBundleBuild[]
            {
                new()
                {
                    assetBundleName = bundleTag,
                    assetNames = assetsInBundle
                }
            };

            if (!Directory.Exists(destination)) Directory.CreateDirectory(destination);
            BuildPipeline.BuildAssetBundles(destination, BuildAssetBundleOptions.None, target);
        }
    }
}