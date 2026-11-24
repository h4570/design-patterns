namespace composite.Packaging
{
    /// <summary>
    /// Leaf - single product which cannot contain other items.
    /// </summary>
    public class Product : IPackageItem
    {

        private readonly decimal _price;
        private readonly decimal _weightKg;

        public Product(string name, decimal price, decimal weightKg)
        {
            Name = name;
            _price = price;
            _weightKg = weightKg;
        }

        public string Name { get; }

        public decimal GetPrice() => _price;

        public decimal GetWeightKg() => _weightKg;

        public int GetProductsCount() => 1;

        public PackageItemSummary Summarize()
        {
            return new PackageItemSummary(Name, "product", GetPrice(), GetWeightKg(), GetProductsCount(), null);
        }

    }
}
