using System.Collections.Generic;
using builder.Db.SqlQueryBuilders;
using builder.Db.SqlQueryDirectors;
using Microsoft.AspNetCore.Mvc;

namespace builder.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class QueryController : ControllerBase
    {

        private readonly SqlQueryDirector _sqlDirector;

        public QueryController()
        {
            _sqlDirector = new SqlQueryDirector();
        }

        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            var mssql = new MssqlQueryBuilder();
            var postgres = new PostgresQueryBuilder();

            _sqlDirector.Builder = mssql;
            var query1 = _sqlDirector.GetNews();

            _sqlDirector.Builder = postgres;
            var query2 = _sqlDirector.GetActions();

            return Ok(new List<string>() { query1, query2 });
        }
    }
}
