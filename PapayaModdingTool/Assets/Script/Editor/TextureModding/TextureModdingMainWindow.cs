using System.Collections.Generic;
using System.IO;
using PapayaModdingTool.Assets.Script.Editor.Universal;
using UnityEditor;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.TextureModding
{
    public class TextureModdingMainWindow : MainWindow
    {
        private Vector2 scrollPos;
        private List<string> items = new List<string>();


        public static void Open(string projectName)
        {
            var window = GetWindow<TextureModdingMainWindow>(Path.GetFileName(projectName));
            window.Initialize(projectName);
            window.Show();
        }
        
        
        private void OnEnable()
        {
            // Populate example items
            for (int i = 1; i <= 50; i++)
                items.Add("Item " + i);
        }

        protected override void OnGUI()
        {
            base.OnGUI();

            // Begin scroll view and capture the scroll position
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(300));

            // Draw list items
            foreach (var item in items)
            {
                GUILayout.Label(item);
            }

            // End scroll view
            EditorGUILayout.EndScrollView();

            // Optional: Add buttons or other controls below
            if (GUILayout.Button("Add Item"))
            {
                items.Add("Item " + (items.Count + 1));
                Repaint();
            }
        }
    }
}