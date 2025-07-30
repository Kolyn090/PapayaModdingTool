using System.IO;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Misc.Paths
{
    public class PredefinedPaths
    {
        public const string ExternalDir = "Papaya_External";
        public const string ExternalTestDir = "Papaya_External_Test";
        public static readonly string ClassDataPath = Path.Combine(Application.streamingAssetsPath, "classdata.tpk");
    }
}