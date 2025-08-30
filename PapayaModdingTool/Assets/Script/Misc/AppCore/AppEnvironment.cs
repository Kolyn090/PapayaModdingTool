using AssetsTools.NET.Extra;
using PapayaModdingTool.Assets.Script.Dispatcher;
using PapayaModdingTool.Assets.Script.Misc.Paths;
using PapayaModdingTool.Assets.Script.Program;

namespace PapayaModdingTool.Assets.Script.Misc.AppCore
{
    public class AppEnvironment
    {
        public EventDispatcher Dispatcher { get; } = new();
        public AssetsManager AssetsManager { get; } = new();
        public AppWrapper Wrapper { get; } = new();
        public AppSettingsManager AppSettingsManager;
        public CommandManager CommandManager { get; } = new();

        public AppEnvironment()
        {
            AssetsManager.LoadClassPackage(PredefinedPaths.ClassDataPath);
            AppSettingsManager = new(Wrapper.JsonSerializer);
        }
    }
}