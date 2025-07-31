using UnityEditor;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.Universal
{
    public abstract class MainWindow : BaseEditorWindow
    {
        protected string _projectName;

        public void Initialize(string projectName)
        {
            _projectName = projectName;
        }

        protected virtual void OnGUI()
        {
            GUILayout.Label(string.Format(ELT("editing_project"), _projectName), EditorStyles.boldLabel);
        }
    }
}
