# Builder pattern

In this example, builder pattern was used for creating SQL queries for different SQL server providers (*Mssql* and *Postgres*).  
Builder is useful for creating things which have many types and properties.  
Please look on **QueryController.cs** to get better view of this implementation.

### Client code
```csharp
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
```

### Director
```csharp
public string GetActions()
{
    _builder.Select("Actions", new List<string>() { "Id", "Type" });
    _builder.Where("Type", "Funny");
    _builder.Limit(100);
    return _builder.GetSQL();
}
```

### Client result (`/query`)
```json
[
  "SELECT [Id], [Name], [Content] FROM [News] WHERE [Id]=1",
  "SELECT \"Id\", \"Type\" FROM \"Actions\" WHERE \"Type\"='Funny' LIMIT 100"
]
```
