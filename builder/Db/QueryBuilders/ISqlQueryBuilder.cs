using System.Collections.Generic;

namespace builder.Db.SqlQueryBuilders
{
    public interface ISqlQueryBuilder
    {
        public void Select(string table, IEnumerable<string> fields);
        public void Where(string field, string value, string operator_ = "=");
        public void Where(string field, int value, string operator_ = "=");
        public void Limit(int qty);
        public string GetSQL();
        public void Reset();
    }
}
