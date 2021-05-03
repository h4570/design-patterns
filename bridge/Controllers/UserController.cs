using bridge.Data;
using bridge.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace bridge.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {

        private readonly IDatabaseProvider _db;

        /// <exception cref="T:System.Exception">Wrong provider</exception>
        public UserController(IOptions<object> configs)
        {
            // If you want to use Postgres, change first parameter.
            _db = Utilities.GetCurrentDbProvider(DbSettings.Mongodb, configs.Value);
        }

        [HttpGet("latest")]
        public ActionResult<User> GetLatestUser()
        {
            var user = _db.GetLatestUser();
            return Ok(user);
        }

    }
}
