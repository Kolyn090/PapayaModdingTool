using System.Collections.Generic;

namespace PapayaModdingTool.Assets.Script.Editor.Atlas2D.Shortcut
{
    public class ShortcutManagerEnabler
    {
        private readonly List<ShortcutManager> _shortcutManagers = new();

        public void AddShortcutManager(ShortcutManager shortcutManager)
        {
            _shortcutManagers.Add(shortcutManager);
        }

        // Only one is enabled at a time
        public void Enable(ShortcutManager shortcutManager)
        {
            if (shortcutManager == null) return;

            foreach (ShortcutManager sm in _shortcutManagers)
            {
                sm.SetEnabled(sm == shortcutManager);
            }
        }
    }
}