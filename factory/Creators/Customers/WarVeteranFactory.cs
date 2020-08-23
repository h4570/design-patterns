using factory.Models.Customers;
using System.Collections.Generic;

namespace factory.Creators.Customers
{
    public class WarVeteranFactory : CustomerFactory
    {

        private readonly List<WarVeteran> _customers;

        public WarVeteranFactory()
        {
            _customers = new List<WarVeteran>();
        }


        public override ICustomer Create()
        {
            return new WarVeteran() { Id = 2, FullName = "Carl Johnson" };
        }

        public override IEnumerable<ICustomer> GetAll()
        {
            return _customers;
        }

    }
}
