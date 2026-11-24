using memento.Resumes;
using Microsoft.AspNetCore.Mvc;

namespace memento.Controllers
{
    public record HeadlineChange(string Headline);
    public record NewSkill(string Skill);
    public record NewVersion(string Label);

    [ApiController]
    [Route("[controller]")]
    public class ResumeController : ControllerBase
    {

        private readonly ResumeEditor _editor;
        private readonly ResumeHistory _history;

        public ResumeController(ResumeEditor editor, ResumeHistory history)
        {
            _editor = editor;
            _history = history;
        }

        [HttpGet]
        public ActionResult GetResume() => Ok(_editor.Read(Show));

        [HttpPut("headline")]
        public ActionResult SetHeadline(HeadlineChange request) => Edit(c => c.SetHeadline(request.Headline));

        [HttpPost("skills")]
        public ActionResult AddSkill(NewSkill request) => Edit(c => c.AddSkill(request.Skill));

        [HttpDelete("skills/{skill}")]
        public ActionResult RemoveSkill(string skill) => Edit(c => c.RemoveSkill(skill));

        [HttpPost("ai-rewrite")]
        public ActionResult AiRewrite()
        {
            _editor.AiRewrite();
            return Ok(_editor.Read(Show));
        }

        [HttpPost("revert")]
        public ActionResult Revert()
        {
            var label = _editor.RevertLastCheckpoint();
            return label == null ? Conflict("Nothing to revert") : Ok(new { Reverted = label, Resume = _editor.Read(Show) });
        }

        [HttpGet("versions")]
        public ActionResult<IReadOnlyList<VersionInfo>> GetVersions() => Ok(_history.List());

        [HttpPost("versions")]
        public ActionResult SaveVersion(NewVersion request)
        {
            try
            {
                var memento = _editor.SaveVersion(request.Label);
                return Ok(new { memento.Id, memento.Label, memento.CreatedAt });
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("versions/{id:guid}/restore")]
        public ActionResult Restore(Guid id)
        {
            try
            {
                var memento = _editor.RestoreVersion(id);
                return Ok(new { Restored = memento.Label, Resume = _editor.Read(Show) });
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }

        /// <summary>
        /// Browser friendly example on a separate resume - shows headline and skills after every step.
        /// </summary>
        [HttpGet("demo")]
        public ActionResult Demo()
        {
            var history = new ResumeHistory();
            var editor = new ResumeEditor(Resume.CreateSample(), history);
            var steps = new List<object>();

            void Step(string action) => steps.Add(editor.Read(c => new { Action = action, c.Headline, Skills = string.Join(", ", c.Skills) }));

            Step("Start");
            var backend = editor.SaveVersion("Backend jobs");
            Step("Save version 'Backend jobs'");

            editor.Edit(c =>
            {
                c.SetHeadline("Team Lead / Senior .NET Developer");
                c.RemoveSkill("Docker");
                c.AddSkill("Mentoring");
                c.AddSkill("Scrum");
            });
            Step("Edit for team lead offers");
            editor.SaveVersion("Team lead jobs");
            Step("Save version 'Team lead jobs'");

            editor.AiRewrite();
            Step("AI rewrite (checkpoint taken automatically)");
            Step($"Revert: {editor.RevertLastCheckpoint()}");

            editor.RestoreVersion(backend.Id);
            Step("Restore version 'Backend jobs'");
            Step($"Revert: {editor.RevertLastCheckpoint()}");

            return Ok(new { Steps = steps, Versions = history.List() });
        }

        private ActionResult Edit(Action<Resume> edit)
        {
            try
            {
                _editor.Edit(edit);
                return Ok(_editor.Read(Show));
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
        }

        private static object Show(Resume resume) => new { resume.FullName, resume.Headline, resume.Skills, resume.Experience };

    }
}
