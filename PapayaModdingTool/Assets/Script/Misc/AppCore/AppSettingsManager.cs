using PapayaModdingTool.Assets.Script.Reader.AppSettings;
using PapayaModdingTool.Assets.Script.Wrapper.Json;
using PapayaModdingTool.Assets.Script.Writer.AppSettings;

namespace PapayaModdingTool.Assets.Script.Misc.AppCore
{
    public class AppSettingsManager
    {
        public AppSettingsReader Reader;
        public AppSettingsWriter Writer;

        public AppSettingsManager(IJsonSerializer jsonSerializer)
        {
            Reader = new(jsonSerializer);
            Writer = new(jsonSerializer);
        }
    }
}