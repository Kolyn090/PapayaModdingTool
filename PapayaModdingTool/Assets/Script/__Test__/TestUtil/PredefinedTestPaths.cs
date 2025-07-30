using System.IO;
using PapayaModdingTool.Assets.Script.Misc.Paths;

namespace PapayaModdingTool.Assets.Script.__Test__.TestUtil
{
    public class PredefinedTestPaths
    {
        public static readonly string DoNotOverridePath = Path.Combine(PredefinedPaths.ExternalTestDir, "__DoNotOverwrite__");
        public static readonly string LabDeskPath = Path.Combine(PredefinedPaths.ExternalTestDir, "__LabDesk__");
    }
}