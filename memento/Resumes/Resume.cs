namespace memento.Resumes
{
    /// <summary>
    /// Immutable, so it can be safely shared between the resume and its snapshots.
    /// </summary>
    public record Experience(string Company, string Role, string Period);

    /// <summary>
    /// What the caretaker is allowed to see - label and date, nothing about the content.
    /// </summary>
    public interface IResumeMemento
    {
        Guid Id { get; }
        string Label { get; }
        DateTime CreatedAt { get; }
    }

    /// <summary>
    /// Originator - the only class which can create a snapshot of its state and restore it.
    /// </summary>
    public class Resume
    {

        private readonly List<string> _skills;
        private readonly List<Experience> _experience;

        public Resume(string fullName, string headline, IEnumerable<string> skills, IEnumerable<Experience> experience)
        {
            FullName = fullName;
            Headline = headline;
            _skills = skills.ToList();
            _experience = experience.ToList();
        }

        public string FullName { get; private set; }
        public string Headline { get; private set; }
        public IReadOnlyList<string> Skills => _skills;
        public IReadOnlyList<Experience> Experience => _experience;

        public void SetHeadline(string headline)
        {
            if (string.IsNullOrWhiteSpace(headline))
                throw new ArgumentException("Headline cannot be empty");
            Headline = headline.Trim();
        }

        public void AddSkill(string skill)
        {
            if (string.IsNullOrWhiteSpace(skill))
                throw new ArgumentException("Skill cannot be empty");
            if (!_skills.Contains(skill, StringComparer.OrdinalIgnoreCase))
                _skills.Add(skill.Trim());
        }

        public void RemoveSkill(string skill)
        {
            if (_skills.RemoveAll(c => c.Equals(skill, StringComparison.OrdinalIgnoreCase)) == 0)
                throw new KeyNotFoundException($"There is no skill '{skill}'");
        }

        public void ReplaceSkills(IEnumerable<string> skills)
        {
            // Materialized first - "skills" can be a lazy query over _skills itself
            var newSkills = skills.ToList();
            _skills.Clear();
            _skills.AddRange(newSkills);
        }

        public void AddExperience(Experience experience) => _experience.Insert(0, experience);

        public IResumeMemento Save(string label) =>
            // Lists are copied - otherwise snapshot and resume would share them and later edits would change the "saved" version
            new Snapshot(label, FullName, Headline, _skills.ToList(), _experience.ToList());

        public void Restore(IResumeMemento memento)
        {
            if (memento is not Snapshot snapshot)
                throw new ArgumentException("This memento was not created by a resume");

            FullName = snapshot.FullName;
            Headline = snapshot.Headline;
            // Copied again - snapshot has to stay untouched, it can be restored many times
            ReplaceSkills(snapshot.Skills);
            _experience.Clear();
            _experience.AddRange(snapshot.Experience);
        }

        /// <summary>
        /// Memento. Private nested class - only Resume can read the saved state,
        /// everyone else sees just IResumeMemento.
        /// </summary>
        private sealed class Snapshot : IResumeMemento
        {

            public Snapshot(string label, string fullName, string headline, List<string> skills, List<Experience> experience)
            {
                Label = label;
                FullName = fullName;
                Headline = headline;
                Skills = skills;
                Experience = experience;
            }

            public Guid Id { get; } = Guid.NewGuid();
            public string Label { get; }
            public DateTime CreatedAt { get; } = DateTime.Now;

            public string FullName { get; }
            public string Headline { get; }
            public IReadOnlyList<string> Skills { get; }
            public IReadOnlyList<Experience> Experience { get; }

        }

        public static Resume CreateSample() => new(
            "Anna Kowalska",
            ".NET Developer",
            new[] { "C#", "ASP.NET Core", "SQL Server", "Docker" },
            new[]
            {
                new Experience("Allegro", "Software Engineer", "2022 - now"),
                new Experience("Comarch", "Junior .NET Developer", "2019 - 2022")
            });

    }
}
