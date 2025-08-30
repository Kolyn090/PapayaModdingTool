namespace PapayaModdingTool.Assets.Script.Program
{
    public interface ICommand
    {
        void Execute();
        void Undo();
    }
}