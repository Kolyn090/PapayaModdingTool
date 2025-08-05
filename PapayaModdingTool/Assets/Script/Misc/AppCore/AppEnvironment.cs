using AssetsTools.NET.Extra;
using PapayaModdingTool.Assets.Script.Dispatcher;
using PapayaModdingTool.Assets.Script.Misc.Paths;

namespace PapayaModdingTool.Assets.Script.Misc.AppCore
{
    public class AppEnvironment
    {
        public EventDispatcher Dispatcher { get; } = new();
        public AssetsManager AssetsManager { get; } = new();
        public AppWrapper Wrapper { get; } = new();
        public AppSettingsManager AppSettingsManager;

        public AppEnvironment()
        {
            AssetsManager.LoadClassPackage(PredefinedPaths.ClassDataPath);
            AppSettingsManager = new(Wrapper.JsonSerializer);
        }
    }
}