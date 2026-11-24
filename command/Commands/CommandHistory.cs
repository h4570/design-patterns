namespace command.Commands
{
    public record HistoryEntry(string Description, DateTime ExecutedAt);

    /// <summary>
    /// Invoker - executes commands and keeps history for undo / redo.
    /// It does not know what commands do - it only calls Execute() and Undo().
    /// </summary>
    public class CommandHistory
    {

        private readonly Stack<(ICatalogCommand Command, DateTime At)> _done = new();
        private readonly Stack<(ICatalogCommand Command, DateTime At)> _undone = new();
        private readonly object _lock = new();

        public void Execute(ICatalogCommand command)
        {
            lock (_lock)
            {
                command.Execute();      // when it throws, nothing lands in history
                _done.Push((command, DateTime.Now));
                _undone.Clear();        // new change makes old "redo" path invalid
            }
        }

        /// <returns>Description of undone command, null when there is nothing to undo</returns>
        public string Undo()
        {
            lock (_lock)
            {
                if (_done.Count == 0) return null;
                var entry = _done.Pop();
                entry.Command.Undo();
                _undone.Push(entry);
                return entry.Command.Description;
            }
        }

        /// <returns>Description of redone command, null when there is nothing to redo</returns>
        public string Redo()
        {
            lock (_lock)
            {
                if (_undone.Count == 0) return null;
                var entry = _undone.Pop();
                entry.Command.Execute();
                _done.Push((entry.Command, DateTime.Now));
                return entry.Command.Description;
            }
        }

        public (IReadOnlyList<HistoryEntry> Done, IReadOnlyList<HistoryEntry> CanRedo) GetHistory()
        {
            lock (_lock)
            {
                return (_done.Select(c => new HistoryEntry(c.Command.Description, c.At)).ToList(),
                        _undone.Select(c => new HistoryEntry(c.Command.Description, c.At)).ToList());
            }
        }

    }
}
