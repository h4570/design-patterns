using System.Collections.Concurrent;

namespace facade.Checkout.Subsystems
{
    /// <summary>
    /// Subsystem - warehouse stock. Products have to be reserved first and committed or released later.
    /// </summary>
    public class InventoryService
    {

        private readonly ConcurrentDictionary<string, (string Name, decimal Price, int Stock)> _products = new()
        {
            ["KB-001"] = ("Mechanical keyboard", 459.00m, 10),
            ["MS-002"] = ("Wireless mouse", 149.00m, 25),
            ["MN-003"] = ("Monitor 27\"", 1299.00m, 3)
        };

        private readonly ConcurrentDictionary<string, IReadOnlyList<OrderLine>> _reservations = new();
        private readonly object _lock = new();

        public decimal GetPrice(string sku)
        {
            if (!_products.TryGetValue(sku, out var product))
                throw new ArgumentException($"Unknown product {sku}");
            return product.Price;
        }

        public IReadOnlyDictionary<string, int> GetStock() =>
            _products.ToDictionary(c => c.Key, c => c.Value.Stock);

        /// <returns>Reservation id</returns>
        public string Reserve(IReadOnlyList<OrderLine> lines)
        {
            lock (_lock)
            {
                foreach (var line in lines)
                {
                    if (!_products.TryGetValue(line.Sku, out var product))
                        throw new ArgumentException($"Unknown product {line.Sku}");
                    if (product.Stock < line.Quantity)
                        throw new InvalidOperationException($"Not enough {product.Name} in stock ({product.Stock} left)");
                }

                foreach (var line in lines)
                {
                    var product = _products[line.Sku];
                    _products[line.Sku] = product with { Stock = product.Stock - line.Quantity };
                }
            }

            var reservationId = $"RES-{Guid.NewGuid().ToString()[..8].ToUpper()}";
            _reservations[reservationId] = lines;
            return reservationId;
        }

        public void Commit(string reservationId) => _reservations.TryRemove(reservationId, out _);

        public void Release(string reservationId)
        {
            if (!_reservations.TryRemove(reservationId, out var lines)) return;

            lock (_lock)
                foreach (var line in lines)
                {
                    var product = _products[line.Sku];
                    _products[line.Sku] = product with { Stock = product.Stock + line.Quantity };
                }
        }

    }
}
