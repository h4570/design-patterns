using composite.Packaging;
using composite.Services;
using Microsoft.AspNetCore.Mvc;

namespace composite.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PackageController : ControllerBase
    {

        private readonly ShippingCostCalculator _shippingCostCalculator;

        public PackageController(ShippingCostCalculator shippingCostCalculator)
        {
            _shippingCostCalculator = shippingCostCalculator;
        }

        /// <summary>
        /// Packs example "home office" order and returns whole package tree with shipping cost.
        /// </summary>
        [HttpGet]
        public ActionResult GetHomeOfficeOrder()
        {
            var package = PackHomeOfficeOrder();

            return Ok(new
            {
                ShippingCost = _shippingCostCalculator.Calculate(package),
                Package = package.Summarize()
            });
        }

        /// <summary>
        /// Same calculator, but used with a single product - no special handling needed.
        /// </summary>
        [HttpGet("single")]
        public ActionResult GetSingleProduct()
        {
            var monitor = new Product("Monitor 27\"", 1299.00m, 6.2m);

            return Ok(new
            {
                ShippingCost = _shippingCostCalculator.Calculate(monitor),
                Package = monitor.Summarize()
            });
        }

        private static IPackageItem PackHomeOfficeOrder()
        {
            var cablesBag = new Box("Antistatic bag", 0.50m, 0.01m)
                .Add(new Product("USB-C cable", 39.99m, 0.05m))
                .Add(new Product("HDMI cable", 29.99m, 0.08m));

            var accessoriesBox = new Box("Small box", 2.00m, 0.15m)
                .Add(new Product("Wireless mouse", 149.00m, 0.09m))
                .Add(new Product("Mechanical keyboard", 459.00m, 1.10m))
                .Add(cablesBag);

            var laptopBox = new Box("Laptop box", 5.00m, 0.40m)
                .Add(new Product("Laptop 14\"", 5499.00m, 1.35m))
                .Add(new Product("Laptop charger", 0m, 0.30m));

            return new Box("Shipping carton", 8.00m, 0.90m)
                .Add(laptopBox)
                .Add(accessoriesBox)
                .Add(new Product("Desk lamp", 199.00m, 1.20m));
        }

    }
}
