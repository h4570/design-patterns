using factory_pattern.Models;

namespace factory_pattern.Data
{

    public interface IDbFactory
    {
        public abstract IDbUser Users();
    }

}
