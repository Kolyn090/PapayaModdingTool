using System.IO;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Misc.Paths
{
    public class PredefinedPaths
    {
        public const string ExternalDir = "Papaya_External";
        public const string ExternalTestDir = "Papaya_External_Test";
        public static readonly string AppSettingsFile = Path.Combine(ExternalDir, "settings.json");
        public static readonly string ClassDataPath = Path.Combine(Application.streamingAssetsPath, "classdata.tpk");
        public static readonly string ProjectsPath = Path.Combine(ExternalDir, "Projects");
        public static readonly string ProjectInfoPath = Path.Combine(ProjectsPath,
                                                                    "{0}", // project name
                                                                    "info.json");
        public static readonly string LocalizationPath = Path.Combine(Application.streamingAssetsPath, "Localization");

        public static readonly string FileTextureFolder = Path.Combine(
            ProjectsPath,
            "{0}", // project name
            "{1}", // file name
            "Texture"
        );
        public static readonly string ExternalFileTextureExportedFolder = Path.Combine(FileTextureFolder, "Exported");
        public static readonly string ExternalFileTextureOwningDumpFolder = Path.Combine(FileTextureFolder, "Owning Dump");
        public static readonly string ExternalFileTextureSourceDumpFolder = Path.Combine(FileTextureFolder, "Source Dump");
        public static readonly string ExternalFileTextureImportedFolder = Path.Combine(FileTextureFolder, "Imported");

        public static readonly string ExternalFileAtlas2DFolder = Path.Combine(FileTextureFolder, "Atlas2D");
        public static readonly string Atlas2DSpritesPanelSaveJson = Path.Combine(ExternalFileAtlas2DFolder, "SpritesPanelSave.json");

        public const string PapayaUnityDir = "Assets/Papaya";
        public static readonly string PapayaTextureDir = Path.Combine(PapayaUnityDir, "Texture");
        public static readonly string PapayaTextureProjectPath = Path.Combine(
            PapayaTextureDir,
            "{0}" // project name
        );
        public static readonly string AssetBundlesPath = Path.Combine(PapayaUnityDir, "AssetBundles");

        public const string PapayaUnityTestDir = "Assets/Papaya_Test";
        public static readonly string PapayaTestTextureDir = Path.Combine(PapayaUnityTestDir, "Texture");

        public const string ExportFolderName = "Papaya_Exported";
    }
}