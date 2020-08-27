using builder.Db.SqlQueryBuilders;
using System.Collections.Generic;

namespace builder.Db.SqlQueryDirectors
{
    public class SqlQueryDirector
    {

        private ISqlQueryBuilder _builder;
        public ISqlQueryBuilder Builder { set { _builder = value; } }

        public string GetNews()
        {
            _builder.Select("News", new List<string>() { "Id", "Name", "Content" });
            _builder.Where("Id", 1);
            return _builder.GetSQL();
        }

        public string GetActions()
        {
            _builder.Select("Actions", new List<string>() { "Id", "Type" });
            _builder.Where("Type", "Funny");
            _builder.Limit(100);
            return _builder.GetSQL();
        }

    }

}
