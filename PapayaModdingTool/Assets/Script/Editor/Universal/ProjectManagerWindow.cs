using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.Universal
{
    public class ProjectManagerWindow : BaseEditorWindow
    {
        [MenuItem("Tools/__ Project Manager", false, 99)]
        public static void ShowWindow()
        {
            GetWindow<ProjectManagerWindow>(ELT("manage_project"));
        }

        private void DeleteProject(string projectPath)
        {
            bool confirm = EditorUtility.DisplayDialog(
                ELT("delete_project"),
                $"{ELT("double_confirm_delete_project")} \"{Path.GetFileName(projectPath)}\"?\n{ELT("cannot_undone")}",
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