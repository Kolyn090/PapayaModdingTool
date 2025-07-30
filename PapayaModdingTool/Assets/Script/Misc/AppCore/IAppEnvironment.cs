namespace PapayaModdingTool.Assets.Script.Misc.AppCore
{
    public interface IAppEnvironment
    {
        AppEnvironment AppEnvironment { get; }

        void Initialize(AppEnvironment appEnvironment);
    }
}