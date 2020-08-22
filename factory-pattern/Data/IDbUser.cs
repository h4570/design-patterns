using factory_pattern.Models;

namespace factory_pattern.Data
{

    public interface IDbUser
    {
        public User GetLatestUser();
        public void Add(User user);
        public void ChangePassword(int userId, string password);
        public void Delete(User user);
    }

}
