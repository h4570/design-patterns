using System;
using System.Collections.Generic;
using System.Linq;
using factory_pattern.Data;
using factory_pattern.Data.Mongodb;
using factory_pattern.Data.Postgres;
using factory_pattern.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace factory_pattern.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {

        private readonly IDbFactory _factory;

        public UserController(IOptions<object> configs)
        {
            _factory = Utilities.GetCurrentDatabase(configs.Value);
        }

        [HttpGet("latest")]
        public ActionResult<User> GetLatestUser()
        {
            var user = _factory.Users().GetLatestUser();
            return Ok(user);
        }

    }
}
