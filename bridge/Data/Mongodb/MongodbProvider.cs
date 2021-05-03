using bridge.Models;
using System.Linq;

namespace bridge.Data.Mongodb
{
    public class MongodbProvider : IDatabaseProvider
    {

        private readonly MongodbConnector _connector;

        public MongodbProvider(MongodbConnector connector)
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
