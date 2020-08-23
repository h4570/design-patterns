using factory.Models.Customers;
using System.Collections.Generic;

namespace factory.Creators.Customers
{
    public class StudentFactory : CustomerFactory
    {

        private readonly List<Student> _customers;

        public StudentFactory()
        {
            _customers = new List<Student>();
        }

        public override ICustomer Create()
        {
            var student = new Student() { Id = 1, FullName = "John Doe" };
            _customers.Add(student);
            return student;
        }

        public override IEnumerable<ICustomer> GetAll()
        {
            return _customers;
        }

    }
}
