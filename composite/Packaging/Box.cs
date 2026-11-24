namespace composite.Packaging
{
    /// <summary>
    /// Composite - box which can contain products and other boxes.
    /// Every operation is delegated to children and combined with box's own data (packaging cost and weight).
    /// </summary>
    public class Box : IPackageItem
    {

        private readonly List<IPackageItem> _items = new();
        private readonly decimal _packagingCost;
        private readonly decimal _packagingWeightKg;

        public Box(string name, decimal packagingCost, decimal packagingWeightKg)
        {
            Name = name;
            _packagingCost = packagingCost;
            _packagingWeightKg = packagingWeightKg;
        }

        public string Name { get; }

        public IReadOnlyList<IPackageItem> Items => _items;

        public Box Add(IPackageItem item)
        {
            if (ReferenceEquals(item, this)) throw new InvalidOperationException("Box cannot contain itself");
            _items.Add(item);
            return this;
        }

        public void Remove(IPackageItem item) => _items.Remove(item);

        public decimal GetPrice() => _packagingCost + _items.Sum(c => c.GetPrice());

        public decimal GetWeightKg() => _packagingWeightKg + _items.Sum(c => c.GetWeightKg());

        public int GetProductsCount() => _items.Sum(c => c.GetProductsCount());

        public PackageItemSummary Summarize()
        {
            var children = _items.Select(c => c.Summarize()).ToList();
            return new PackageItemSummary(Name, "box", GetPrice(), GetWeightKg(), GetProductsCount(), children);
        }

    }
}
