using bridge.Models;
using System.Collections.Generic;

namespace bridge.Data.Postgres
{
    public class PostgresDbContext
    {

        public PostgresDbContext(object configs)
        {
            Users = new List<User>()
            {
                new User(){ Id=1, Username="John", Password="Doe"},
                new User(){ Id=2, Username="Mark", Password="Smith"},
                new User(){ Id=3, Username="Joshua", Password="Anderson"}
            };
        }

        /// <summary>
        /// Replace this property with entity framework DbSet<>
        /// </summary>
        public List<User> Users { get; set; }

    }
}
