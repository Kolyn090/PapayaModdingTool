using System.Collections.Generic;

namespace PapayaModdingTool.Assets.Script.Program
{
    public class CommandManager
    {
        private readonly Stack<ICommand> _history = new();

        public void ExecuteCommand(ICommand command)
        {
            command.Execute();
            _history.Push(command);
        }

        public void Undo()
        {
            if (_history.Count > 0)
            {
                ICommand lastCommand = _history.Pop();
                lastCommand.Undo();
            }
        }
    }
}