using factory.Models.Customers;
using System.Collections.Generic;

namespace factory.Creators.Customers
{
    public abstract class CustomerFactory
    {
        public abstract ICustomer Create();
        public abstract IEnumerable<ICustomer> GetAll();

        public decimal AddDiscount(ICustomer customer, decimal price)
        {
            var disc = 100 - customer.DiscountPercentage;
            return price * (disc / (decimal)100);
        }
    }
}
