using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PapayaModdingTool.Assets.Script.DataStruct.EditorWindow;
using PapayaModdingTool.Assets.Script.Editor.Atlas2D;
using PapayaModdingTool.Assets.Script.Editor.TextureModding;
using PapayaModdingTool.Assets.Script.Misc.Paths;
using PapayaModdingTool.Assets.Script.Reader.ProjectUtil;
using PapayaModdingTool.Assets.Script.Writer.ProjectUtil;
using UnityEditor;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.Universal
{
    public class RecentProjectsWindow : BaseEditorWindow
    {
        private static EditorWindowType _editorWindowType;
        private string _newProjectName = "";
        private int _selectedIndex = -1;
        private List<string> _recentProjects = null;
        private List<string> _renderRecentProjects = null;

        public static void ShowWindow(string title, EditorWindowType editorWindowType)
        {
            Initialize();
            GetWindow<RecentProjectsWindow>(title);
            _editorWindowType = editorWindowType;
        }

        private void OnGUI()
        {
            GUILayout.Label(ELT("select_project"), EditorStyles.boldLabel);
            if (_recentProjects == null)
            {
                _recentProjects = RecentProjectsFinder.FindRecentProjects();
                _renderRecentProjects = _recentProjects.Select(x => Path.GetFileName(x)).ToList();
            }
            _selectedIndex = EditorGUILayout.Popup(ELT("found_projects"), _selectedIndex, _renderRecentProjects.ToArray());

            GUILayout.Space(20);

            EditorGUI.BeginDisabledGroup(_recentProjects.Count == 0 || _selectedIndex < 0);
            if (GUILayout.Button(ELT("open_project")) && _selectedIndex >= 0)
            {
                string projectName = _recentProjects[_selectedIndex];
                OpenEditorForProject(projectName);
            }
            EditorGUI.EndDisabledGroup();

            GUILayout.Space(20);

            bool isNameEmpty = string.IsNullOrWhiteSpace(_newProjectName);
            bool nameExists = _recentProjects.Contains(_newProjectName);
            bool canCreate = !isNameEmpty && !nameExists;
            // Show warning if the name already exists
            if (nameExists)
            {
                EditorGUILayout.HelpBox(ELT("project_already_exists"), MessageType.Warning);
            }
            GUILayout.Label(ELT("or_create_new"), EditorStyles.boldLabel);
            _newProjectName = EditorGUILayout.TextField("", _newProjectName);
            EditorGUI.BeginDisabledGroup(!canCreate);
            if (GUILayout.Button(ELT("create_new_project")))
            {
                CreateNewProject();
            }
            GUILayout.Label(string.Format(ELT("fyi_save_project"), PredefinedPaths.ProjectsPath), EditorStyles.miniBoldLabel);
            EditorGUI.EndDisabledGroup();
        }

        private void OpenEditorForProject(string projectName)
        {
            GoToNextWindow(projectName);
            Close();
        }

        private void CreateNewProject()
        {
            string newProjectPath = Path.Combine(PredefinedPaths.ProjectsPath, _newProjectName);
            NewProjectCreator creator = new();
            creator.CreateNewProject(newProjectPath, _appEnvironment.Wrapper.JsonSerializer);
            OpenEditorForProject(newProjectPath);
        }

        private void GoToNextWindow(string projectName)
        {
            switch (_editorWindowType)
            {
                case EditorWindowType.TextureModding:
                    TextureModdingMainWindow.Open(projectName);
                    break;
                case EditorWindowType.Atlas2D:
                    Atlas2DMainWindow.Open(projectName);
                    break;
                default:
                    throw new NotImplementedException($"{_editorWindowType}'s main window has not been implemented yet.");
            }
        }
    }
}
