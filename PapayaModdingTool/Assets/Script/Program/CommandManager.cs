using System.Collections.Generic;

namespace PapayaModdingTool.Assets.Script.Program
{
    public class CommandManager
    {
        private readonly Stack<ICommand> _undoStack = new();
        private readonly Stack<ICommand> _redoStack = new();
        private readonly int _maxHistory;

        public CommandManager(int maxHistory = 100)
        {
            _maxHistory = maxHistory;
        }

        public void ExecuteCommand(ICommand command)
        {
            command.Execute();
            _undoStack.Push(command);

            // Clear redo stack when new action occurs
            _redoStack.Clear();

            // Enforce history limit
            if (_undoStack.Count > _maxHistory)
            {
                // Remove oldest command (bottom of stack)
                var tempStack = new Stack<ICommand>(_undoStack);
                tempStack.Pop(); // discard oldest
                _undoStack.Clear();
                foreach (var cmd in tempStack)
                    _undoStack.Push(cmd);
            }
        }

        public void Undo()
        {
            if (_undoStack.Count > 0)
            {
                ICommand lastCommand = _undoStack.Pop();
                lastCommand.Undo();
                _redoStack.Push(lastCommand);
            }
        }

        public void Redo()
        {
            if (_redoStack.Count > 0)
            {
                ICommand redoCommand = _redoStack.Pop();
                redoCommand.Execute();
                _undoStack.Push(redoCommand);
            }
        }

        public void ClearHistory()
        {
            _undoStack.Clear();
            _redoStack.Clear();
        }
    }
}