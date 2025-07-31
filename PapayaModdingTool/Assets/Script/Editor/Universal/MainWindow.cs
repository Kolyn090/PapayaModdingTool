using System.IO;
using UnityEditor;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.Universal
{
    public abstract class MainWindow : EditorWindow
    {
        protected string _projectName;

        public void Initialize(string projectName)
        {
            _projectName = projectName;
        }

        protected virtual void OnGUI()
        {
            GUILayout.Label($"Editing Project: {_projectName}", EditorStyles.boldLabel);
        }
    }
}