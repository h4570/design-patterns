using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace builder.Db.SqlQueryBuilders
{
    public class PostgresQueryBuilder : ISqlQueryBuilder
    {

        private string _selectQuery = string.Empty;
        private string _whereQuery = string.Empty;
        private string _limitQuery = string.Empty;

        private bool WasSelectUsed { get { return !_selectQuery.Equals(string.Empty); } }

        public void Select(string table, IEnumerable<string> fields)
        {
            fields = fields.Prepend(string.Empty);
            var fieldsQuery = fields.Aggregate((a, b) => $"{a}, \"{b}\"").Substring(2);
            _selectQuery = $"SELECT {fieldsQuery} FROM \"{table}\"";
        }

        public void Where(string field, string value, string operator_ = "=")
        {
            _whereQuery = $" WHERE \"{field}\"{operator_}'{value}'";
        }

        public void Where(string field, int value, string operator_ = "=")
        {
            _whereQuery = $" WHERE \"{field}\"{operator_}{value}";
        }

        public void Limit(int qty)
        {
            _limitQuery = $" LIMIT {qty}";
        }

        public string GetSQL()
        {
            if (WasSelectUsed)
            {
                var query = new StringBuilder(string.Empty);
                query.Append(_selectQuery);
                query.Append(_whereQuery);
                query.Append(_limitQuery);
                return query.ToString();
            }
            else throw new Exception("'SELECT' must be provided to perform valid SQL query. Please use Select method.");
        }

        public void Reset()
        {
            _selectQuery = string.Empty;
            _whereQuery = string.Empty;
            _limitQuery = string.Empty;
        }

    }
}
