using Microsoft.AspNetCore.Mvc;
using singleton.Services;

namespace singleton.Controllers
{
    [ApiController]
    [Route("logger-test")]
    public class LoggerTestController : ControllerBase
    {

        private readonly LoggerService _loggerService;

        public LoggerTestController(LoggerService loggerService)
        {
            _loggerService = loggerService;
        }

        [HttpGet]
        public ActionResult Get()
        {
            var currentSecondInString = DateTime.Now.Second.ToString();
            _loggerService.Add(currentSecondInString);
            return Ok($"done - log will be saved to {_loggerService.FilePath} within {LoggerService.SaveInterval.TotalSeconds} s");
        }

        /// <summary>
        /// Saves pending logs immediately and returns the last lines of the file.
        /// </summary>
        [HttpGet("flush")]
        public ActionResult<IEnumerable<string>> Flush()
        {
            _loggerService.Flush();
            if (!System.IO.File.Exists(_loggerService.FilePath)) return Ok(Array.Empty<string>());
            return Ok(System.IO.File.ReadLines(_loggerService.FilePath).TakeLast(10));
        }

    }
}
