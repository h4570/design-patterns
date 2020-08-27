using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using prototype.Models;
using prototype.Repositories;

namespace prototype.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DevicesController : ControllerBase
    {

        private readonly DevicePrototypeRegistry _deviceProtoRegistry;

        public DevicesController()
        {
            _deviceProtoRegistry = new DevicePrototypeRegistry();
        }

        [HttpGet]
        public ActionResult<IEnumerable<IDevice>> Get()
        {
            var smartphone = new Smartphone() { Id = 1, Name = "Super phone" };
            _deviceProtoRegistry.Add(smartphone);

            var copy = _deviceProtoRegistry.GetCopyById(1);
            copy.Id = 2;

            return Ok(new List<IDevice>() { smartphone, copy });
        }

    }
}
