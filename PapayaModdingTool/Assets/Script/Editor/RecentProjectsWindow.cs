using System.Collections.Generic;
using System.IO;
using System.Linq;
using PapayaModdingTool.Assets.Script.Misc.Paths;
using PapayaModdingTool.Assets.Script.Reader.ProjectUtil;
using UnityEditor;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor
{
    public class RecentProjectsWindow : EditorWindow
    {
        private string _newProjectName = "";
        private int selectedIndex = -1;

        public static void ShowWindow(string title)
        {
            GetWindow<RecentProjectsWindow>(title);
        }

        private void OnGUI()
        {
            GUILayout.Label(EL.T("select_recent_project"), EditorStyles.boldLabel);
            List<string> recentProjects = RecentProjectsFinder.FindRecentProjects();
            List<string> renderRecentProjects = recentProjects.Select(x => Path.GetFileName(x)).ToList();
            selectedIndex = EditorGUILayout.Popup(EL.T("recent_projects"), selectedIndex, renderRecentProjects.ToArray());

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
            MainWindow.Open(projectName);

            // Close this window
            Close();
        }

        private void CreateNewProject()
        {
            Directory.CreateDirectory(Path.Combine(PredefinedPaths.ProjectsPath, _newProjectName));
        }
    }
}