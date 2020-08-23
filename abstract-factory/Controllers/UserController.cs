using System;
using System.Collections.Generic;
using System.Linq;
using abstract_factory.Data;
using abstract_factory.Data.Mongodb;
using abstract_factory.Data.Postgres;
using abstract_factory.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace abstract_factory.Controllers
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
