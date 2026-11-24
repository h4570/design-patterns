namespace composite.Packaging
{
    /// <summary>
    /// Component - common interface for single products (leafs) and boxes (composites).
    /// Client code does not need to know if it is talking to a single product or to a whole box tree.
    /// </summary>
    public interface IPackageItem
    {
        public string Name { get; }
        public decimal GetPrice();
        public decimal GetWeightKg();
        public int GetProductsCount();
        public PackageItemSummary Summarize();
    }
}
