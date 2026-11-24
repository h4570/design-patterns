namespace command.Commands
{
    /// <summary>
    /// Command - a request turned into an object. Thanks to that it can be stored in history, undone and redone.
    /// </summary>
    public interface ICatalogCommand
    {
        string Description { get; }
        void Execute();
        void Undo();
    }
}
