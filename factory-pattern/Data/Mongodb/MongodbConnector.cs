using factory_pattern.Models;
using System.Collections.Generic;

namespace factory_pattern.Data.Mongodb
{
    public class MongodbConnector
    {

        public MongodbConnector(object configs)
        {
            Users = new List<User>()
            {
                new User(){ Id=1, Username="Stanley", Password="Ken"},
                new User(){ Id=2, Username="Josh", Password="Hernandez"},
                new User(){ Id=3, Username="Jack", Password="Hamilton"}
            };
        }

        /// <summary>
        /// Replace this property with mongodb logic
        /// </summary>
        public List<User> Users { get; set; }

    }
}
