using abstract_factory.Models;

namespace abstract_factory.Data.Mongodb
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
