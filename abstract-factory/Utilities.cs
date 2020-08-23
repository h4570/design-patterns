using abstract_factory.Data;
using abstract_factory.Data.Mongodb;
using abstract_factory.Data.Postgres;

namespace abstract_factory
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
