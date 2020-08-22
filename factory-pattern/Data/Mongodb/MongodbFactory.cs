using factory_pattern.Models;

namespace factory_pattern.Data.Mongodb
{
    public class MongodbFactory : IDbFactory
    {

        private readonly MongodbUser _users;

        public MongodbFactory(MongodbConnector connector)
        {
            _users = new MongodbUser(connector);
        }

        public User GetLatestUser()
        {
            throw new System.NotImplementedException();
        }

        public IDbUser Users() { return _users; }

    }
}
