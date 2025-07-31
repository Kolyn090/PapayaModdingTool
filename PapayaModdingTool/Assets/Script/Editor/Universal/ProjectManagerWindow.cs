using System;
using System.IO;
using PapayaModdingTool.Assets.Script.Misc.Paths;
using UnityEditor;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.Universal
{
    public class ProjectManagerWindow : BaseEditorWindow
    {
        private string _deleteProjectName = "";

        [MenuItem("Tools/__ Project Manager", false, 99)]
        public static void ShowWindow()
        {
            GetWindow<ProjectManagerWindow>(ELT("manage_project"));
        }

        private void OnGUI()
        {
            Debug.Log("GUI called");
            GUILayout.Label(ELT("delete_project"), EditorStyles.boldLabel);
            GUILayout.Label(ELT("enter_project_name_to_delete"), EditorStyles.whiteLabel);
            _deleteProjectName = EditorGUILayout.TextField("", _deleteProjectName);
            bool isNameEmpty = string.IsNullOrWhiteSpace(_deleteProjectName);
            bool nameExists = Directory.Exists(Path.Combine(PredefinedPaths.ProjectsPath, _deleteProjectName));
            bool canDelete = !isNameEmpty && nameExists;

            if (!nameExists)
            {
                EditorGUILayout.HelpBox(string.Format(ELT("project_name_dne"), _deleteProjectName), MessageType.Warning);
            }

            EditorGUI.BeginDisabledGroup(!canDelete);
            if (GUILayout.Button(ELT("delete_project_button")))
            {
                DeleteProject(Path.Combine(PredefinedPaths.ProjectsPath, _deleteProjectName));
            }
            EditorGUI.EndDisabledGroup();
        }

        private void DeleteProject(string projectPath)
        {
            bool confirm = EditorUtility.DisplayDialog(
                ELT("delete_project"),
                $"{string.Format(ELT("double_confirm_delete_project"), Path.GetFileName(projectPath))}\n{ELT("cannot_undone")}",
                ELT("delete_project_button"), ELT("delete_cancel_button")
            );

            if (!confirm)
                return;

            if (Directory.Exists(projectPath))
            {
                try
                {
                    Directory.Delete(projectPath, true);
                    File.Delete(projectPath + ".meta");

                    EditorUtility.DisplayDialog(
                        ELT("project_deleted_title"),
                        string.Format(ELT("project_deleted_message"), Path.GetFileName(projectPath)),
                        ELT("ok")
                    );
                }
                catch (Exception ex)
                {
                    Debug.LogError($"{ELT("fail_to_delete_project")}: " + ex.Message);
                    EditorUtility.DisplayDialog(ELT("error"), ELT("fail_to_delete_project"), ELT("ok"));
                }
            }
        }
    }
}