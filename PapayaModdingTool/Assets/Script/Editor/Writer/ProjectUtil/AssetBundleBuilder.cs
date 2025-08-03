using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.Writer.ProjectUtil
{
    public class AssetBundleBuilder
    {
        // If search path is empty, search entire unity instead
        public static void BuildAllAssetBundles(string destination, BuildTarget target)
        {
            // Ensure destination directory exists
            if (!Directory.Exists(destination))
                Directory.CreateDirectory(destination);

            // Build the bundle
            BuildPipeline.BuildAssetBundles(destination, BuildAssetBundleOptions.None, target);
        }
    }
}