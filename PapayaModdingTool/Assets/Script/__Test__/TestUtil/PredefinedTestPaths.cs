using System.IO;
using PapayaModdingTool.Assets.Script.Misc.Paths;

namespace PapayaModdingTool.Assets.Script.__Test__.TestUtil
{
    public class PredefinedTestPaths
    {
        public static readonly string TestResPath = Path.Combine(PredefinedPaths.ExternalTestDir, "TestResources");
        public static readonly string DoNotOverridePath = Path.Combine(TestResPath, "__DoNotOverwrite__");
        public static readonly string LabDeskPath = Path.Combine(TestResPath, "__LabDesk__");
        public static readonly string UnityDoNotOverridePath = Path.Combine(PredefinedPaths.PapayaUnityTestDir, "__DoNotOverwrite__");
        public static readonly string UnityLabDeskPath = Path.Combine(PredefinedPaths.PapayaUnityTestDir, "__LabDesk__");
    }
}