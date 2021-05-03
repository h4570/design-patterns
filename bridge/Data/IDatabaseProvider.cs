using bridge.Models;

namespace bridge.Data
{

    public interface IDatabaseProvider
    {
        public User GetLatestUser();
        public void Add(User user);
        public void ChangePassword(int userId, string password);
        public void Delete(User user);
    }

}
