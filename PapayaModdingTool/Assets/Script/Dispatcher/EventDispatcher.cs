using System.Collections.Generic;
using PapayaModdingTool.Assets.Script.Event;
using PapayaModdingTool.Assets.Script.EventListener;

namespace PapayaModdingTool.Assets.Script.Dispatcher
{
    public class EventDispatcher
    {
        private readonly List<IAppEventListener> _listeners = new();
        public void Register(IAppEventListener listener) => _listeners.Add(listener);
        public void Unregister(IAppEventListener listener) => _listeners.Remove(listener);

        public void Dispatch(AppEvent e)
        {
            var snapshot = _listeners.ToArray();

            foreach (var listener in snapshot)
            {
                listener.OnEvent(e);
            }
        }
    }
}