using System;
using bridge.Data;
using bridge.Data.Mongodb;
using bridge.Data.Postgres;

namespace bridge
{

    public enum DbSettings
    {
        Postgres = 0,
        Mongodb = 1
    }

    public static class Utilities
    {
        /// <exception cref="T:System.Exception">Wrong provider</exception>
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

    }

}
