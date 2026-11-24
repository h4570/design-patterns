using System.Globalization;
using command.Catalog;

namespace command.Commands
{

    public class ChangePriceCommand : ICatalogCommand
    {
        private readonly ProductCatalog _catalog;
        private readonly string _sku;
        private readonly decimal _newPrice;
        private decimal _oldPrice;

        public ChangePriceCommand(ProductCatalog catalog, string sku, decimal newPrice)
        {
            _catalog = catalog;
            _sku = sku;
            _newPrice = newPrice;
        }

        public string Description => string.Create(CultureInfo.InvariantCulture, $"Change price of {_sku} to {_newPrice:0.00}");

        public void Execute()
        {
            // State needed for undo is remembered at execution time, not in constructor
            _oldPrice = _catalog.Get(_sku).Price;
            _catalog.SetPrice(_sku, _newPrice);
        }

        public void Undo() => _catalog.SetPrice(_sku, _oldPrice);
    }

    public class CategoryDiscountCommand : ICatalogCommand
    {
        private readonly ProductCatalog _catalog;
        private readonly string _category;
        private readonly int _percent;
        private readonly Dictionary<string, decimal> _oldPrices = new();

        public CategoryDiscountCommand(ProductCatalog catalog, string category, int percent)
        {
            if (percent is <= 0 or >= 100)
                throw new ArgumentException("Discount has to be between 1 and 99 percent", nameof(percent));
            _catalog = catalog;
            _category = category;
            _percent = percent;
        }

        public string Description => $"{_percent}% discount on {_category}";

        public void Execute()
        {
            _oldPrices.Clear();
            foreach (var product in _catalog.InCategory(_category))
            {
                _oldPrices[product.Sku] = product.Price;
                // Rounded to .99 - nobody likes prices like 3999.20
                var discounted = Math.Floor(product.Price * (100 - _percent) / 100) - 0.01m;
                _catalog.SetPrice(product.Sku, discounted);
            }
            if (_oldPrices.Count == 0)
                throw new KeyNotFoundException($"Category '{_category}' has no products");
        }

        public void Undo()
        {
            foreach (var (sku, price) in _oldPrices)
                _catalog.SetPrice(sku, price);
        }
    }

    public class PublishProductCommand : ICatalogCommand
    {
        private readonly ProductCatalog _catalog;
        private readonly string _sku;
        private readonly bool _publish;
        private bool _wasPublished;

        public PublishProductCommand(ProductCatalog catalog, string sku, bool publish = true)
        {
            _catalog = catalog;
            _sku = sku;
            _publish = publish;
        }

        public string Description => $"{(_publish ? "Publish" : "Hide")} {_sku}";

        public void Execute()
        {
            _wasPublished = _catalog.Get(_sku).IsPublished;
            _catalog.SetPublished(_sku, _publish);
        }

        public void Undo() => _catalog.SetPublished(_sku, _wasPublished);
    }

    /// <summary>
    /// Macro command - many commands executed (and undone) as one step.
    /// </summary>
    public class MacroCommand : ICatalogCommand
    {
        private readonly IReadOnlyList<ICatalogCommand> _commands;

        public MacroCommand(string description, params ICatalogCommand[] commands)
        {
            Description = description;
            _commands = commands;
        }

        public string Description { get; }

        public void Execute()
        {
            var executed = new Stack<ICatalogCommand>();
            try
            {
                foreach (var command in _commands)
                {
                    command.Execute();
                    executed.Push(command);
                }
            }
            catch
            {
                // All or nothing - roll back what was already done
                while (executed.Count > 0) executed.Pop().Undo();
                throw;
            }
        }

        // Undo in reverse order
        public void Undo()
        {
            foreach (var command in _commands.Reverse())
                command.Undo();
        }
    }

}
