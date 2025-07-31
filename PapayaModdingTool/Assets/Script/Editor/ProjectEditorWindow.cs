using UnityEditor;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor
{
    public class ProjectEditorWindow : EditorWindow
    {
        private string projectName;

        public static void Open(string projectName)
        {
            var window = GetWindow<ProjectEditorWindow>("Project Editor");
            window.projectName = projectName;
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label($"Editing Project: {projectName}", EditorStyles.boldLabel);

            // You can load data or UI for the selected project here
        }
    }
}