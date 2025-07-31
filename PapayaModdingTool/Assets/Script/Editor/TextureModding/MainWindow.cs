using UnityEditor;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor
{
    public class MainWindow : EditorWindow
    {
        private string _projectName;

        public static void Open(string projectName)
        {
            var window = GetWindow<MainWindow>("Texture Modding");
            window._projectName = projectName;
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label($"Editing Project: {_projectName}", EditorStyles.boldLabel);

            
        }
    }
}