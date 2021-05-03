using bridge.Models;
using System.Linq;

namespace bridge.Data.Postgres
{
    public class PostgresProvider : IDatabaseProvider
    {

        private readonly PostgresDbContext _context;

        public PostgresProvider(PostgresDbContext context)
        {
            _context = context;
        }

        public User GetLatestUser()
        {
            return _context.Users.Last();
        }

        public void Add(User user)
        {
            _context.Users.Add(user);
        }

        public void Delete(User user)
        {
            _context.Users.Remove(user);
        }

        public void ChangePassword(int userId, string password)
        {
            var usr = _context.Users.Single(c => c.Id == userId);
            usr.Password = password;
        }

    }
}
