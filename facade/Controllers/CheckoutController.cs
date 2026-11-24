using facade.Checkout;
using facade.Checkout.Subsystems;
using Microsoft.AspNetCore.Mvc;

namespace facade.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CheckoutController : ControllerBase
    {

        private readonly CheckoutFacade _checkout;
        private readonly InventoryService _inventory;

        // Without facade this controller would need all five subsystems
        // and would have to know the whole checkout flow.
        public CheckoutController(CheckoutFacade checkout, InventoryService inventory)
        {
            _checkout = checkout;
            _inventory = inventory;
        }

        [HttpPost]
        public ActionResult<CheckoutResult> PlaceOrder(OrderRequest request)
        {
            var result = _checkout.PlaceOrder(request);
            return result.Success ? Ok(result) : UnprocessableEntity(result);
        }

        /// <summary>
        /// Browser friendly example - places sample order with valid card.
        /// </summary>
        [HttpGet("demo")]
        public ActionResult<CheckoutResult> Demo() => PlaceOrder(CreateSampleOrder("4111111111111111"));

        /// <summary>
        /// Browser friendly example - card is declined, reserved products go back to the warehouse.
        /// </summary>
        [HttpGet("demo-declined")]
        public ActionResult<CheckoutResult> DemoDeclined() => PlaceOrder(CreateSampleOrder("4000000000000002"));

        [HttpGet("stock")]
        public ActionResult<IReadOnlyDictionary<string, int>> GetStock() => Ok(_inventory.GetStock());

        private static OrderRequest CreateSampleOrder(string cardNumber) => new(
            "john.doe@example.com",
            new[] { new OrderLine("KB-001", 1), new OrderLine("MS-002", 2) },
            new Address("John Doe", "Marszałkowska 1", "Warszawa", "00-001", "PL"),
            cardNumber);

    }
}
