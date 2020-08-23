namespace abstract_factory.Data.Postgres
{
    public class PostgresFactory : IDbFactory
    {

        private readonly PostgresUser _users;

        public PostgresFactory(PostgresDbContext context)
        {
            _users = new PostgresUser(context);
        }

        public IDbUser Users() { return _users; }

    }
}
