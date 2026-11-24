# Bridge pattern

In this example bridge pattern was used to manage database provider.  
As you can see, there are two implementations of IDatabaseProvider: *PostgreSQL* and *Mongodb*  
To test this example, you can run "**/user/latest**" endpoint and compare results after changing first parameter of `Utilities.GetCurrentDbProvider()` function.

### Client code
```csharp
public class UserController : ControllerBase
{
    private readonly IDatabaseProvider _db;

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
```

### Bridge between abstraction and implementations
```csharp
public static IDatabaseProvider GetCurrentDbProvider(DbSettings dbProvider, object configs)
{
    if (dbProvider == DbSettings.Postgres)
    {
        var postgresContext = new PostgresDbContext(configs);
        return new PostgresProvider(postgresContext);
    }
    else if (dbProvider == DbSettings.Mongodb)
    {
        var mongoConnector = new MongodbConnector(configs);
        return new MongodbProvider(mongoConnector);
    }
    else throw new Exception("Wrong provider");
}
```

### Result (`/user/latest`)
```json
{ "id": 3, "username": "Joshua", "password": "Anderson" }
```
