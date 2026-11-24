# Memento pattern

In this example memento pattern was used in an **online resume (CV) builder with saved versions**.  
User keeps few versions of one resume ("Backend jobs", "Team lead jobs") and can go back to any of them.  
Before every risky change - "improve with AI" button or restoring an old version - an automatic checkpoint is taken, so the change can be reverted.  
`Resume` creates a snapshot of its own state (`Save`) and restores it (`Restore`). `ResumeHistory` stores the snapshots, **but cannot read what is inside** -
memento is a private nested class, outside of `Resume` it is visible only as `IResumeMemento` with a label and a date.  
To test this example, run "**/resume/demo**" endpoint, or edit the shared resume:  
`PUT /resume/headline`, `POST /resume/skills`, `POST /resume/ai-rewrite`, `POST /resume/revert`, `POST /resume/versions`, `GET /resume/versions`, `POST /resume/versions/{id}/restore`.  
Please look on **Resume.cs** and **ResumeHistory.cs** to get better view of this implementation.

## Participants

- `Resume` - **originator**, the only class which knows how to save and restore its state
- `Resume.Snapshot` (private) / `IResumeMemento` - **memento**, full state inside, only label and date outside
- `ResumeHistory` - **caretaker**, keeps named versions forever and the last 5 autosaves, never looks inside
- `ResumeEditor` - takes checkpoints before risky changes
- `ResumeController` - **client**

### Originator and memento
```csharp
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

/// Private nested class - only Resume can read the saved state
private sealed class Snapshot : IResumeMemento { ... }
```
`Experience` is an immutable record, so copying the list is enough - there is no need to clone every item.

### Caretaker
```csharp
public void Checkpoint(IResumeMemento memento)
{
    lock (_lock)
    {
        _autosaves.AddLast(memento);
        if (_autosaves.Count > MaxAutosaves)   // snapshots cost memory - keep only the last few
            _autosaves.RemoveFirst();
    }
}
```

### Client code
```csharp
public void AiRewrite()
{
    lock (_lock)
    {
        _history.Checkpoint(_resume.Save("Before AI rewrite"));
        _resume.SetHeadline($"Results-driven {_resume.Headline} | Passionate innovator | Rockstar ninja");
        _resume.ReplaceSkills(Buzzwords.Concat(_resume.Skills).Take(6));
    }
}
```

### Result (`/resume/demo`)
| Step | Headline | Skills |
|---|---|---|
| Start | .NET Developer | C#, ASP.NET Core, SQL Server, Docker |
| Save version 'Backend jobs' | .NET Developer | C#, ASP.NET Core, SQL Server, Docker |
| Edit for team lead offers | Team Lead / Senior .NET Developer | C#, ASP.NET Core, SQL Server, Mentoring, Scrum |
| Save version 'Team lead jobs' | Team Lead / Senior .NET Developer | C#, ASP.NET Core, SQL Server, Mentoring, Scrum |
| AI rewrite (checkpoint taken automatically) | Results-driven Team Lead / Senior .NET Developer \| Passionate innovator \| Rockstar ninja | Synergy, Blockchain, Thought leadership, 10x mindset, C#, ASP.NET Core |
| Revert: Before AI rewrite | Team Lead / Senior .NET Developer | C#, ASP.NET Core, SQL Server, Mentoring, Scrum |
| Restore version 'Backend jobs' | .NET Developer | C#, ASP.NET Core, SQL Server, Docker |
| Revert: Before restoring 'Backend jobs' | Team Lead / Senior .NET Developer | C#, ASP.NET Core, SQL Server, Mentoring, Scrum |

## Memento vs Command

Both give you undo. Command remembers **how to reverse one change** (old price of one product), memento remembers **the whole state**.  
Memento is simpler when changes are hard to reverse (AI rewrite changes many fields at once), but every snapshot costs memory - that is why the caretaker keeps only 5 autosaves.  
See the command example in this repository to compare.
