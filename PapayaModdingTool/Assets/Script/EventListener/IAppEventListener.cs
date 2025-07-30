using PapayaModdingTool.Assets.Script.Event;

namespace PapayaModdingTool.Assets.Script.EventListener
{
    public interface IAppEventListener
    {
        void OnEvent(AppEvent e);
    }
}