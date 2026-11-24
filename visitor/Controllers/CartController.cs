using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using visitor.Cart;

namespace visitor.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CartController : ControllerBase
    {

        public const decimal FreeShippingFrom = 300m;

        /// <summary>
        /// Summary of your own cart - items need "type": physical, ebook, giftCard or installation.
        /// </summary>
        [HttpPost("summary")]
        public ActionResult Summary(IReadOnlyList<ICartItem> items)
        {
            if (items == null || items.Count == 0)
                return BadRequest("Cart is empty");
            return Ok(CreateSummary(items));
        }

        [HttpGet("demo")]
        public ActionResult Demo()
        {
            var items = SampleCart.Create();
            return Ok(new { Items = items, Summary = CreateSummary(items) });
        }

        private static object CreateSummary(IReadOnlyList<ICartItem> items)
        {
            // Three different operations on the same items - each one is a visitor
            var vat = items.Select(c => c.Accept(new VatCalculator())).ToList();
            var deliveries = items.Select(c => c.Accept(new DeliveryPlanner())).ToList();
            var accounting = items.Select(c => c.Accept(new AccountingExport())).ToList();

            var parcelWeight = deliveries.Where(c => c.Method == "Parcel").Sum(c => c.WeightKg);
            var goodsValue = vat.Where(c => c.Rate != "NP").Sum(c => c.Gross);

            return new
            {
                Total = vat.Sum(c => c.Gross),
                VatByRate = vat.GroupBy(c => c.Rate).Select(g => new { Rate = g.Key, Net = g.Sum(c => c.Net), Vat = g.Sum(c => c.Vat) }),
                Shipping = parcelWeight == 0 ? "No parcel needed"
                    : goodsValue >= FreeShippingFrom ? string.Create(CultureInfo.InvariantCulture, $"Parcel {parcelWeight} kg - free shipping")
                    : string.Create(CultureInfo.InvariantCulture, $"Parcel {parcelWeight} kg - 14.99 PLN"),
                Deliveries = deliveries,
                Accounting = accounting
            };
        }

    }
}
