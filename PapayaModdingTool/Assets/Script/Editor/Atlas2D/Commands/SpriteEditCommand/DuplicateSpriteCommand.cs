using System;
using System.Collections.Generic;
using System.IO;
using PapayaModdingTool.Assets.Script.DataStruct.TextureData;
using PapayaModdingTool.Assets.Script.Misc.Paths;
using PapayaModdingTool.Assets.Script.Program;
using PapayaModdingTool.Assets.Script.Writer.Atlas2D;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.Atlas2D.Commands.SpriteEditCommand
{
    public class DuplicateSpriteCommand : ICommand
    {
        private readonly string _projectName;
        private readonly string _fileFolderName;
        private readonly string _savePath;
        private readonly SpritesPanelSaver _saver;
        private readonly List<SpriteButtonData> _selected;

        private readonly List<string> _duplicatePaths = new();

        public DuplicateSpriteCommand(List<SpriteButtonData> selected,
                                    string projectName,
                                    string fileFolderName,
                                    string savePath,
                                    SpritesPanelSaver saver)
        {
            _projectName = projectName;
            _fileFolderName = fileFolderName;
            _selected = selected;
            _savePath = savePath;
            _saver = saver;
        }

        public void Execute()
        {
            foreach (SpriteButtonData data in _selected)
            {
                DuplicateSprite(_projectName, _fileFolderName, _savePath, data, _saver);
            }
        }

        public void Undo()
        {
            string importedPath = string.Format(PredefinedPaths.ExternalFileTextureImportedFolder, _projectName, _fileFolderName);
            foreach (string path in _duplicatePaths)
            {
                try
                {
                    if (File.Exists(path))
                        File.Delete(path);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to delete file at {path}: {ex.Message}");
                }
            }

            // Save the original selected, should clear out duplication
            _saver.Save(_savePath, importedPath, _selected);
        }

        // !!! Only for Imported
        private void DuplicateSprite(string projectName, string fileFolderName, string savePath, SpriteButtonData data, SpritesPanelSaver saver)
        {
            string importedPath = string.Format(PredefinedPaths.ExternalFileTextureImportedFolder, projectName, fileFolderName);
            string determinedImagePath = PathUtils.ToLongPath(Path.Combine(importedPath, data.originalLabel));
            string actualPath = PathUtils.FindImagePath(determinedImagePath);
            if (actualPath != null)
            {
                string[] duplicates = PathUtils.DuplicateFile(actualPath, 1);
                string duplicated = duplicates[0];
                _duplicatePaths.Add(duplicated);
                // Also copy properties in the save file
                saver.CopyAfterDuplication(savePath, importedPath, data.originalLabel, Path.GetFileNameWithoutExtension(duplicated));
                Debug.Log($"Successfully duplicated {data.originalLabel}. All changes will be applied after you reopen this Texture.");
            }
            else
            {
                Debug.Log($"Failed to duplicate {data.originalLabel}. Either it's not an Imported sprite or you renamed it.");
            }
        }
    }
}