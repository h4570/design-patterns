namespace command.Catalog
{
    public class Product
    {
        public string Sku { get; init; }
        public string Name { get; init; }
        public string Category { get; init; }
        public decimal Price { get; set; }
        public bool IsPublished { get; set; }

        public Product Snapshot() => (Product)MemberwiseClone();
    }

    /// <summary>
    /// Receiver - knows how to do the real work. Commands call it, but it knows nothing about commands or undo.
    /// </summary>
    public class ProductCatalog
    {

        private readonly Dictionary<string, Product> _products;

        public ProductCatalog()
        {
            _products = new[]
            {
                new Product { Sku = "LAP-14", Name = "Laptop 14\"", Category = "Electronics", Price = 4999m, IsPublished = true },
                new Product { Sku = "MON-27", Name = "Monitor 27\" 4K", Category = "Electronics", Price = 1899m, IsPublished = true },
                new Product { Sku = "KB-001", Name = "Mechanical keyboard", Category = "Accessories", Price = 459m, IsPublished = true },
                new Product { Sku = "MS-002", Name = "Wireless mouse", Category = "Accessories", Price = 149m, IsPublished = true },
                new Product { Sku = "BF-BUNDLE", Name = "Black Friday bundle: laptop + mouse", Category = "Bundles", Price = 4799m, IsPublished = false },
            }.ToDictionary(c => c.Sku, StringComparer.OrdinalIgnoreCase);
        }

        public IReadOnlyList<Product> All() => _products.Values.Select(c => c.Snapshot()).ToList();

        /// <exception cref="KeyNotFoundException">Unknown SKU</exception>
        public Product Get(string sku) => _products.TryGetValue(sku, out var product)
            ? product
            : throw new KeyNotFoundException($"Product '{sku}' does not exist");

        public IReadOnlyList<Product> InCategory(string category) => _products.Values
            .Where(c => string.Equals(c.Category, category, StringComparison.OrdinalIgnoreCase))
            .ToList();

        public void SetPrice(string sku, decimal price)
        {
            if (price <= 0) throw new ArgumentException("Price has to be greater than 0", nameof(price));
            Get(sku).Price = price;
        }

        public void SetPublished(string sku, bool isPublished) => Get(sku).IsPublished = isPublished;

    }
}
