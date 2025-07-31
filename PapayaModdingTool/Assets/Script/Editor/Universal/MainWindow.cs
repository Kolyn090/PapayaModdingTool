using System.IO;
using UnityEditor;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.Universal
{
    public abstract class MainWindow : BaseEditorWindow
    {
        private string _projectPath;
        public string ProjectPath => _projectPath;
        public string ProjectName => Path.GetFileName(_projectPath);

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
