using System.IO;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Misc.Paths
{
    public class PredefinedPaths
    {
        public const string ExternalDir = "Papaya_External";
        public const string ExternalTestDir = "Papaya_External_Test";
        public static readonly string ClassDataPath = Path.Combine(Application.streamingAssetsPath, "classdata.tpk");
        public static readonly string ProjectsPath = Path.Combine(ExternalDir, "Projects");
        public static readonly string LocalizationPath = Path.Combine(Application.streamingAssetsPath, "Localization");


        public const string PapayaUnityDir = "Assets/Papaya";
        public static readonly string PapayaTextureDir = Path.Combine(PapayaUnityDir, "Texture");
    }
}