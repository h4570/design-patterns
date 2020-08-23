using abstract_factory.Models;

namespace abstract_factory.Data
{

    public interface IDbUser
    {
        public User GetLatestUser();
        public void Add(User user);
        public void ChangePassword(int userId, string password);
        public void Delete(User user);
    }

}
