using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace PapayaModdingTool.Assets.Script.Editor
{
    public class RecentProjectsWindow : EditorWindow
    {
        private int selectedIndex = -1;
        private List<string> recentProjects = new()
        {
            "Project A",
            "Project B",
            "Project C"
        };

        [MenuItem("Tools/01 Texture Modding")]
        public static void ShowWindow()
        {
            GetWindow<RecentProjectsWindow>("01 Texture Modding");
        }

        private void OnGUI()
        {
            GUILayout.Label(EL.T("select_recent_project"), EditorStyles.boldLabel);

            selectedIndex = EditorGUILayout.Popup(EL.T("recent_projects"), selectedIndex, recentProjects.ToArray());

            GUILayout.Space(20);

            EditorGUI.BeginDisabledGroup(recentProjects.Count == 0 || selectedIndex < 0);

            if (GUILayout.Button(EL.T("open_project")) && selectedIndex >= 0)
            {
                string projectName = recentProjects[selectedIndex];
                OpenEditorForProject(projectName);
            }

            EditorGUI.EndDisabledGroup();

            GUILayout.Space(20);

            if (GUILayout.Button(EL.T("create_new_project")))
            {
                CreateNewProject();
            }
        }

        private void OpenEditorForProject(string projectName)
        {
            // Open the other EditorWindow
            ProjectEditorWindow.Open(projectName);

            // Close this window
            Close();
        }

        private void CreateNewProject()
        {

        }
    }
}