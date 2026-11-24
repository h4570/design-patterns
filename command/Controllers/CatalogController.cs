using command.Catalog;
using command.Commands;
using Microsoft.AspNetCore.Mvc;

namespace command.Controllers
{
    public record PriceChange(string Sku, decimal Price);
    public record CategoryDiscount(string Category, int Percent);

    /// <summary>
    /// Client - creates commands and gives them to the invoker (CommandHistory).
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class CatalogController : ControllerBase
    {

        private readonly ProductCatalog _catalog;
        private readonly CommandHistory _history;

        public CatalogController(ProductCatalog catalog, CommandHistory history)
        {
            _catalog = catalog;
            _history = history;
        }

        [HttpGet]
        public ActionResult<IReadOnlyList<Product>> GetProducts() => Ok(_catalog.All());

        [HttpPost("price")]
        public ActionResult ChangePrice(PriceChange request) =>
            Run(() => new ChangePriceCommand(_catalog, request.Sku, request.Price));

        [HttpPost("discount")]
        public ActionResult Discount(CategoryDiscount request) =>
            Run(() => new CategoryDiscountCommand(_catalog, request.Category, request.Percent));

        [HttpPost("publish/{sku}")]
        public ActionResult Publish(string sku) => Run(() => new PublishProductCommand(_catalog, sku));

        [HttpPost("hide/{sku}")]
        public ActionResult Hide(string sku) => Run(() => new PublishProductCommand(_catalog, sku, publish: false));

        [HttpPost("black-friday")]
        public ActionResult BlackFriday() => Run(() => CreateBlackFriday(_catalog));

        [HttpPost("undo")]
        public ActionResult Undo()
        {
            var undone = _history.Undo();
            return undone == null ? Conflict("Nothing to undo") : Ok(new { Undone = undone, Products = _catalog.All() });
        }

        [HttpPost("redo")]
        public ActionResult Redo()
        {
            var redone = _history.Redo();
            return redone == null ? Conflict("Nothing to redo") : Ok(new { Redone = redone, Products = _catalog.All() });
        }

        [HttpGet("history")]
        public ActionResult GetHistory()
        {
            var (done, canRedo) = _history.GetHistory();
            return Ok(new { Done = done, CanRedo = canRedo });
        }

        /// <summary>
        /// Browser friendly example - runs a whole scenario on a separate catalog and shows prices after every step.
        /// </summary>
        [HttpGet("demo")]
        public ActionResult Demo()
        {
            var catalog = new ProductCatalog();
            var history = new CommandHistory();
            var steps = new List<object>();

            void Step(string action)
            {
                steps.Add(new
                {
                    Action = action,
                    // Only products visible in the shop
                    Prices = catalog.All().Where(c => c.IsPublished).ToDictionary(c => c.Sku, c => c.Price)
                });
            }

            Step("Start");
            history.Execute(new ChangePriceCommand(catalog, "MON-27", 1799m));
            Step("Execute: change price of MON-27");
            history.Execute(CreateBlackFriday(catalog));
            Step("Execute: Black Friday macro");
            Step($"Undo: {history.Undo()}");
            Step($"Redo: {history.Redo()}");
            Step($"Undo: {history.Undo()}");
            Step($"Undo: {history.Undo()}");

            return Ok(steps);
        }

        private static MacroCommand CreateBlackFriday(ProductCatalog catalog) => new(
            "Black Friday",
            new CategoryDiscountCommand(catalog, "Electronics", 20),
            new CategoryDiscountCommand(catalog, "Accessories", 30),
            new PublishProductCommand(catalog, "BF-BUNDLE"));

        private ActionResult Run(Func<ICatalogCommand> createCommand)
        {
            try
            {
                var command = createCommand();
                _history.Execute(command);
                return Ok(new { Executed = command.Description, Products = _catalog.All() });
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
        }

    }
}
