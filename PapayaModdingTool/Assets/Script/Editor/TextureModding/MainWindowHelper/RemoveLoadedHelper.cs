using System;
using System.Collections.Generic;
using PapayaModdingTool.Assets.Script.DataStruct.FileRead;
using PapayaModdingTool.Assets.Script.Editor.Writer.Universal;
using PapayaModdingTool.Assets.Script.Wrapper.Json;
using UnityEditor;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.TextureModding.MainWindowHelper
{
    public class RemoveLoadedHelper
    {
        public Func<string, string> ELT;
        public Func<List<string>> GetLoadedPaths;
        public Action SetLoadedPathsChangedToTrue;
        public Func<IJsonSerializer> GetJsonSerializer;
        public Func<string> GetProjectName;
        private string _removeLoadedPath;
        private ProjectRemover _projectRemover;

        public void CreateRemoveLoadedPanel()
        {
            GUILayout.Label(ELT("remove_loaded"), EditorStyles.boldLabel);
            _removeLoadedPath = EditorGUILayout.TextField("", _removeLoadedPath);
            bool isRemoveValid = GetLoadedPaths().Contains(_removeLoadedPath);
            if (!isRemoveValid)
            {
                EditorGUILayout.HelpBox(ELT("enter_exact_path_to_delete"), MessageType.Info);
            }
            EditorGUI.BeginDisabledGroup(!isRemoveValid);
            if (GUILayout.Button(ELT("remove_file")))
            {
                RemoveTypedFile();
                SetLoadedPathsChangedToTrue();
            }
            EditorGUI.EndDisabledGroup();
        }

        private void RemoveTypedFile()
        {
            _projectRemover ??= new(GetJsonSerializer());
            _projectRemover.RemoveFileFromProject(_removeLoadedPath, GetProjectName(), LoadType.Texture);
        }
    }
}