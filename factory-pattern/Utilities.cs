using factory_pattern.Data;
using factory_pattern.Data.Mongodb;
using factory_pattern.Data.Postgres;

namespace factory_pattern
{

    public enum DbSetting
    {
        Postgres = 0,
        Mongodb = 1
    }

    public static class Constants
    {
        public static DbSetting CURRENT_DATABASE = DbSetting.Postgres;
    }

    public static class Utilities
    {

        public static IDbFactory GetCurrentDatabase(object configs)
        {
            if (Constants.CURRENT_DATABASE == DbSetting.Postgres)
            {
                var mongoConnector = new MongodbConnector(configs);
                return new MongodbFactory(mongoConnector);
            }
            else
            {
                var postgresContext = new PostgresDbContext(configs);
                return new PostgresFactory(postgresContext);
            }
        }

    }

}
