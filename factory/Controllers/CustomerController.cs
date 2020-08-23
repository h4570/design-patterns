using System;
using System.Collections.Generic;
using System.Linq;
using factory.Creators.Customers;
using factory.Models.Customers;
using Microsoft.AspNetCore.Mvc;

namespace factory.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CustomerController : ControllerBase
    {

        private readonly StudentFactory _studentsFactory;
        private readonly WarVeteranFactory _warVeteransFactory;

        public CustomerController()
        {
            _studentsFactory = new StudentFactory();
            _warVeteransFactory = new WarVeteranFactory();
        }

        [HttpGet]
        public IEnumerable<Tuple<ICustomer, decimal>> CreateCustomersAndCalcDiscounts()
        {
            var result = new List<Tuple<ICustomer, decimal>>();

            decimal applePrice = 10;

            var veteran = _warVeteransFactory.Create();
            var student = _studentsFactory.Create();

            var veteranDiscount = _warVeteransFactory.AddDiscount(veteran, applePrice);
            var studentDiscount = _studentsFactory.AddDiscount(student, applePrice);

            result.Add(Tuple.Create(veteran, veteranDiscount));
            result.Add(Tuple.Create(student, studentDiscount));

            return result;
        }

    }
}
