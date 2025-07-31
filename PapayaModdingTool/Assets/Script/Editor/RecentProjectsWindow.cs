using System.Collections.Generic;
using System.IO;
using PapayaModdingTool.Assets.Script.Misc.AppCore;
using PapayaModdingTool.Assets.Script.Misc.Paths;
using UnityEditor;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor
{
    public class RecentProjectsWindow : EditorWindow
    {
        private readonly AppEnvironment _appEnvironment = new();
        private string _newProjectName = "";
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

            bool isNameEmpty = string.IsNullOrWhiteSpace(_newProjectName);
            bool nameExists = recentProjects.Contains(_newProjectName);
            bool canCreate = !isNameEmpty && !nameExists;
            // Show warning if the name already exists
            if (nameExists)
            {
                EditorGUILayout.HelpBox(EL.T("project_already_exists"), MessageType.Warning);
            }
            GUILayout.Label(EL.T("or_create_new"), EditorStyles.boldLabel);
            _newProjectName = EditorGUILayout.TextField("", _newProjectName);
            EditorGUI.BeginDisabledGroup(!canCreate);
            if (GUILayout.Button(EL.T("create_new_project")))
            {
                CreateNewProject();
            }
            GUILayout.Label(string.Format(EL.T("fyi_save_project"), PredefinedPaths.ProjectsPath), EditorStyles.miniBoldLabel);
            EditorGUI.EndDisabledGroup();
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
            Directory.CreateDirectory(Path.Combine(PredefinedPaths.ProjectsPath, _newProjectName));
        }
    }
}