using PapayaModdingTool.Assets.Script.Program;
using UnityEngine;
using UEvent = UnityEngine.Event;

namespace PapayaModdingTool.Assets.Script.Editor.Atlas2D.Shortcut
{
    public class ShortcutManager
    {
        private bool _isEnabled = false;
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (!_isEnabled && value)
                {
                    _enabler.Enable(this);
                }
                _isEnabled = value;
            }
        }
        private readonly CommandManager _commandManager;
        private ShortcutManagerEnabler _enabler;

        public ShortcutManager(CommandManager commandManager)
        {
            _commandManager = commandManager;
        }

        public void AssignEnabler(ShortcutManagerEnabler enabler)
        {
            _enabler = enabler;
        }

        public void HandleEvent(UEvent e)
        {
            if (!_isEnabled)
                return;

            // Shift + Ctrl + Z
            if (e.type == EventType.KeyDown &&
                e.shift && (e.control || e.command) &&
                e.keyCode == KeyCode.Z)
            {
                _commandManager.Redo();
            }
            // Ctrl + Z
            else if (e.type == EventType.KeyDown &&
                    (e.control || e.command) &&
                    e.keyCode == KeyCode.Z)
            {
                _commandManager.Undo();
            }
            e.Use();
        }
    }
}