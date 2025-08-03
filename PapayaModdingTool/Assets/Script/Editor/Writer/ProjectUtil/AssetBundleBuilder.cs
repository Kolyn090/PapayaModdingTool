using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.Writer.ProjectUtil
{
    public class AssetBundleBuilder
    {
        // If search path is empty, search entire unity instead
        public static void BuildAllAssetBundles(string bundleTag, string destination, BuildTarget target, string searchPath = "")
        {
            // Make sure the search path is within Assets/
            if (!string.IsNullOrWhiteSpace(searchPath) && !searchPath.StartsWith("Assets/"))
            {
                Debug.LogError("Search path must start with 'Assets/'");
                return;
            }

            string[] assetPaths = null;
            // Find all asset paths with the given bundle tag
            if (string.IsNullOrWhiteSpace(searchPath))
            {
                assetPaths = AssetDatabase.GetAllAssetPaths();
            }
            else
            {
                assetPaths = AssetDatabase.FindAssets("", new[] { searchPath });
            }
            string[] assetsInBundle = null;

            if (string.IsNullOrWhiteSpace(searchPath))
            {
                assetsInBundle = assetPaths
                    .Where(path => AssetImporter.GetAtPath(path)?.assetBundleName == bundleTag)
                    .ToArray();
            }
            else
            {
                assetsInBundle = assetPaths
                    .Where(path => AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(path))?.assetBundleName == bundleTag)
                    .ToArray();
            }

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