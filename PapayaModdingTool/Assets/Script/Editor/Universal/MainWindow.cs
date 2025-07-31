using UnityEditor;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.Universal
{
    public abstract class MainWindow : BaseEditorWindow
    {
        protected string _projectPath;

        public void Initialize(string projectPath)
        {
            _projectPath = projectPath;
        }

        protected virtual void OnGUI()
        {
            GUILayout.Label(string.Format(ELT("editing_project"), _projectPath), EditorStyles.boldLabel);
        }
    }
}
