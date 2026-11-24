namespace memento.Resumes
{
    /// <summary>
    /// Application service used by the controller - edits the resume and asks the caretaker to keep snapshots.
    /// One lock for both, because resume and its history are shared between requests.
    /// </summary>
    public class ResumeEditor
    {

        private static readonly string[] Buzzwords = { "Synergy", "Blockchain", "Thought leadership", "10x mindset" };

        private readonly Resume _resume;
        private readonly ResumeHistory _history;
        private readonly object _lock = new();

        public ResumeEditor(Resume resume, ResumeHistory history)
        {
            _resume = resume;
            _history = history;
        }

        public T Read<T>(Func<Resume, T> read)
        {
            lock (_lock) return read(_resume);
        }

        public void Edit(Action<Resume> edit)
        {
            lock (_lock) edit(_resume);
        }

        public IResumeMemento SaveVersion(string label)
        {
            if (string.IsNullOrWhiteSpace(label))
                throw new ArgumentException("Version needs a label");
            lock (_lock)
            {
                var memento = _resume.Save(label);
                _history.SaveVersion(memento);
                return memento;
            }
        }

        /// <summary>
        /// Simulated "improve my resume with AI" button - result is often not what you wanted,
        /// so a checkpoint is taken automatically before it.
        /// </summary>
        public void AiRewrite()
        {
            lock (_lock)
            {
                _history.Checkpoint(_resume.Save("Before AI rewrite"));
                _resume.SetHeadline($"Results-driven {_resume.Headline} | Passionate innovator | Rockstar ninja");
                _resume.ReplaceSkills(Buzzwords.Concat(_resume.Skills).Take(6));
            }
        }

        /// <returns>Label of the restored checkpoint, null when there is nothing to revert</returns>
        public string RevertLastCheckpoint()
        {
            lock (_lock)
            {
                var checkpoint = _history.PopCheckpoint();
                if (checkpoint == null) return null;
                _resume.Restore(checkpoint);
                return checkpoint.Label;
            }
        }

        public IResumeMemento RestoreVersion(Guid id)
        {
            lock (_lock)
            {
                var memento = _history.Find(id);
                // Restoring is a risky change too - it can be reverted with RevertLastCheckpoint()
                _history.Checkpoint(_resume.Save($"Before restoring '{memento.Label}'"));
                _resume.Restore(memento);
                return memento;
            }
        }

    }
}
