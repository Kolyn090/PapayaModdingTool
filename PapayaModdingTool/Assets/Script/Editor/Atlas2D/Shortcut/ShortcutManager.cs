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

        public void SetEnabled(bool value) => _isEnabled = value;

        private readonly CommandManager _commandManager;
        private ShortcutManagerEnabler _enabler;
        private IShortcutSavable _shortcutSavable;
        private IShortcutNavigable _shortcutNavigable;

        public ShortcutManager(CommandManager commandManager)
        {
            _commandManager = commandManager;
        }

        public void AssignEnabler(ShortcutManagerEnabler enabler)
        {
            _enabler = enabler;
        }

        public void AssignSavable(IShortcutSavable shortcutSavable)
        {
            _shortcutSavable = shortcutSavable;
        }

        public void AssignNavigable(IShortcutNavigable shortcutNavigable)
        {
            _shortcutNavigable = shortcutNavigable;
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
                Debug.Log("Redo.");
                e.Use();
            }
            // Ctrl + Z
            else if (e.type == EventType.KeyDown &&
                    (e.control || e.command) &&
                    e.keyCode == KeyCode.Z)
            {
                _commandManager.Undo();
                Debug.Log("Undo.");
                e.Use();
            }
            // Ctrl + S
            else if (e.type == EventType.KeyDown &&
                    (e.control || e.command) &&
                    e.keyCode == KeyCode.S)
            {
                _shortcutSavable?.OnShortcutSave();
                Debug.Log("Save.");
                e.Use();
            }
            // Use arrow keys
            else if (e.type == EventType.KeyDown &&
                    IsArrowKey(e.keyCode))
            {
                _shortcutNavigable?.OnShortNavigate(e.keyCode);
                Debug.Log($"{e.keyCode}");
                e.Use();
            }
        }

        private bool IsArrowKey(KeyCode key)
        {
            return key == KeyCode.UpArrow ||
                key == KeyCode.DownArrow ||
                key == KeyCode.LeftArrow ||
                key == KeyCode.RightArrow;
        }
    }
}