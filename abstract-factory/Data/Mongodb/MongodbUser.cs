using abstract_factory.Models;
using System.Linq;

namespace abstract_factory.Data.Mongodb
{
    public class MongodbUser : IDbUser
    {

        private readonly MongodbConnector _connector;

        public MongodbUser(MongodbConnector connector)
        {
            _connector = connector;
        }

        public User GetLatestUser()
        {
            return _connector.Users.Last();
        }

        public void Add(User user)
        {
            _connector.Users.Add(user);
        }

        public void Delete(User user)
        {
            _connector.Users.Remove(user);
        }

        public void ChangePassword(int userId, string password)
        {
            var usr = _connector.Users.Single(c => c.Id == userId);
            usr.Password = password;
        }

    }
}
