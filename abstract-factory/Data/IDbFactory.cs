using abstract_factory.Models;

namespace abstract_factory.Data
{

    public interface IDbFactory
    {
        public abstract IDbUser Users();
    }

}
