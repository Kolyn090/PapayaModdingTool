using System.Collections.Generic;
using PapayaModdingTool.Assets.Script.DataStruct.TextureData;
using PapayaModdingTool.Assets.Script.Program;

namespace PapayaModdingTool.Assets.Script.Editor.Atlas2D.Commands.SpriteEditCommand
{
    public class UndoTrashbinCommand : ICommand
    {
        private readonly TrashbinCommand _trashbinCommand;

        public UndoTrashbinCommand(List<SpriteButtonData> selected,
                                    string projectName,
                                    string fileFolderName)
        {
            _trashbinCommand = new(selected, projectName, fileFolderName);
        }

        public void Execute()
        {
            _trashbinCommand.Undo();
        }

        public void Undo()
        {
            _trashbinCommand.Execute();
        }
    }
}