namespace memento.Resumes
{
    public record VersionInfo(Guid Id, string Label, DateTime CreatedAt, bool IsAutosave);

    /// <summary>
    /// Caretaker - keeps mementos and decides how many of them to keep,
    /// but cannot read or change what is inside them.
    /// </summary>
    public class ResumeHistory
    {

        public const int MaxAutosaves = 5;

        private readonly List<IResumeMemento> _versions = new();
        private readonly LinkedList<IResumeMemento> _autosaves = new();
        private readonly object _lock = new();

        /// <summary>
        /// Version saved by the user on purpose, e.g. "for team lead jobs". Kept forever.
        /// </summary>
        public void SaveVersion(IResumeMemento memento)
        {
            lock (_lock) _versions.Add(memento);
        }

        /// <summary>
        /// Automatic checkpoint before a risky change. Only the last few are kept.
        /// </summary>
        public void Checkpoint(IResumeMemento memento)
        {
            lock (_lock)
            {
                _autosaves.AddLast(memento);
                if (_autosaves.Count > MaxAutosaves)
                    _autosaves.RemoveFirst();
            }
        }

        /// <returns>Last checkpoint (removed from history), null when there is none</returns>
        public IResumeMemento PopCheckpoint()
        {
            lock (_lock)
            {
                var last = _autosaves.Last?.Value;
                if (last != null) _autosaves.RemoveLast();
                return last;
            }
        }

        public IResumeMemento Find(Guid id)
        {
            lock (_lock)
                return _versions.Concat(_autosaves).FirstOrDefault(c => c.Id == id)
                    ?? throw new KeyNotFoundException($"There is no version {id}");
        }

        public IReadOnlyList<VersionInfo> List()
        {
            lock (_lock)
                return _versions.Select(c => new VersionInfo(c.Id, c.Label, c.CreatedAt, false))
                    .Concat(_autosaves.Reverse().Select(c => new VersionInfo(c.Id, c.Label, c.CreatedAt, true)))
                    .ToList();
        }

    }
}
