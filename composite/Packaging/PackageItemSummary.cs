namespace composite.Packaging
{
    public record PackageItemSummary(
        string Name,
        string Type,
        decimal Price,
        decimal WeightKg,
        int ProductsCount,
        IReadOnlyList<PackageItemSummary> Items);
}
