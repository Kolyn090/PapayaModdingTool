using System.Collections.Generic;
using System.IO;
using PapayaModdingTool.Assets.Script.DataStruct.TextureData;
using PapayaModdingTool.Assets.Script.Misc.Paths;
using PapayaModdingTool.Assets.Script.Program;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.Atlas2D.Commands.SpriteEditCommand
{
    public class TrashbinCommand : ICommand
    {
        private readonly string _projectName;
        private readonly string _fileFolderName;
        private readonly List<SpriteButtonData> _selected;

        public TrashbinCommand(List<SpriteButtonData> selected,
                                string projectName,
                                string fileFolderName)
        {
            _projectName = projectName;
            _fileFolderName = fileFolderName;
            _selected = selected;
        }

        public void Execute()
        {
            foreach (SpriteButtonData data in _selected)
            {
                MoveSpriteToTrashBin(_projectName, _fileFolderName, data);
            }
        }

        public void Undo()
        {
            foreach (SpriteButtonData data in _selected)
            {
                UndoTrashbin(_projectName, _fileFolderName, data);
            }
        }

        // !!! Only for Imported
        private void MoveSpriteToTrashBin(string projectName, string fileFolderName, SpriteButtonData data)
        {
            string trashbinPath = PathUtils.ToLongPath(string.Format(PredefinedPaths.ExternalFileTextureTrashbinFolder, projectName, fileFolderName));
            if (!Directory.Exists(trashbinPath))
                Directory.CreateDirectory(trashbinPath);

            // ! Assuming all images
            // ! Assuming no two images have the same name but with different extension
            string importedPath = string.Format(PredefinedPaths.ExternalFileTextureImportedFolder, projectName, fileFolderName);
            string determinedImagePath = PathUtils.ToLongPath(Path.Combine(importedPath, data.originalLabel));
            string actualPath = PathUtils.FindImagePath(determinedImagePath);
            if (actualPath != null)
            {
                // This image exists, move it to trashbin
                string destinationPath = Path.Combine(trashbinPath, Path.GetFileName(actualPath));
                PathUtils.MoveFileSafe(actualPath, destinationPath);
                data.isInTrashbin = true;
                Debug.Log($"Moved {data.originalLabel} to trashbin. All changes will be applied after you reopen this Texture.");
            }
            else
            {
                Debug.Log($"Failed to move {data.originalLabel} to trashbin. Either it's not an Imported sprite or you renamed it.");
            }
        }

        // !!! Only for Imported
        private void UndoTrashbin(string projectName, string fileFolderName, SpriteButtonData data)
        {
            string trashbinPath = PathUtils.ToLongPath(string.Format(PredefinedPaths.ExternalFileTextureTrashbinFolder, projectName, fileFolderName));
            if (!Directory.Exists(trashbinPath))
                return;

            string imageInTrashbin = Path.Combine(trashbinPath, data.originalLabel);
            string actualPath = PathUtils.FindImagePath(imageInTrashbin);
            if (actualPath != null)
            {
                string importedPath = PathUtils.ToLongPath(string.Format(PredefinedPaths.ExternalFileTextureImportedFolder, projectName, fileFolderName));
                string destinationPath = Path.Combine(importedPath, Path.GetFileName(actualPath));
                PathUtils.MoveFileSafe(actualPath, destinationPath);
                data.isInTrashbin = false;
                Debug.Log($"Recovered {data.originalLabel} from trashbin.");
            }
            else
            {
                Debug.Log($"Failed to undo trashbin for {data.originalLabel}. Either it's not an Imported sprite or it's not in trashbin.");
            }
        }
    }
}