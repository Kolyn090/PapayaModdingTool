using PapayaModdingTool.Assets.Script.Wrapper.Json;

namespace PapayaModdingTool.Assets.Script.Misc.AppCore
{
    public class AppWrapper
    {
        public NewtonsoftJsonSerializer JsonSerializer { get; } = new();
    }
}