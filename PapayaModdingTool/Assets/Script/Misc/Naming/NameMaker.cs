using PapayaModdingTool.Assets.Script.DataStruct.FileRead;

namespace PapayaModdingTool.Assets.Script.Misc.Naming
{
    public class NameMaker
    {
        public static string AssetBundleTag(string projectName, string fileName, LoadType loadType)
        {
            return string.Join('_', new string[] {
                projectName, loadType.ToString().ToLower(), fileName
            });
        }
    }
}
