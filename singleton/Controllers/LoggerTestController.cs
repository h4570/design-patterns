using Microsoft.AspNetCore.Mvc;
using singleton.Services;
using System;

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
            return Ok("done");
        }

    }
}
