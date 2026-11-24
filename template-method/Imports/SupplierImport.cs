using System.Collections.Concurrent;

namespace template_method.Imports
{
    public record ImportedProduct(string Sku, string Name, decimal NetPricePln, int Stock, string Ean = null);

    public record ImportReport(string Supplier, int Rows, int Imported, int Skipped, IReadOnlyList<string> Errors);

    public class ProductRepository
    {

        private readonly ConcurrentDictionary<string, ImportedProduct> _products = new();

        public void Upsert(ImportedProduct product) => _products[product.Sku] = product;
        public IReadOnlyList<ImportedProduct> All() => _products.Values.OrderBy(c => c.Sku).ToList();

    }

    /// <summary>
    /// Abstract class with the template method. Every supplier sends products in a different format,
    /// but the import always goes through the same steps in the same order.
    /// </summary>
    public abstract class SupplierImport
    {

        private readonly ProductRepository _repository;

        protected SupplierImport(ProductRepository repository)
        {
            _repository = repository;
        }

        public abstract string Key { get; }
        public abstract string Supplier { get; }
        public abstract string SampleFile { get; }

        /// <summary>
        /// Template method - not virtual, the algorithm cannot be changed, only its steps.
        /// </summary>
        public ImportReport Run(string content)
        {
            var rows = ReadRows(content);
            var errors = new List<string>();
            int imported = 0, skipped = 0;

            for (var i = 0; i < rows.Count; i++)
            {
                var rowNumber = i + 1;
                try
                {
                    var product = Adjust(Map(rows[i]));
                    var error = Validate(product);
                    if (error != null)
                    {
                        errors.Add($"Row {rowNumber}: {error}");
                        continue;
                    }
                    if (ShouldSkip(product))
                    {
                        skipped++;
                        continue;
                    }
                    _repository.Upsert(product);
                    imported++;
                }
                catch (Exception ex) when (ex is FormatException or KeyNotFoundException or InvalidOperationException)
                {
                    errors.Add($"Row {rowNumber}: {ex.Message}");
                }
            }

            var report = new ImportReport(Supplier, rows.Count, imported, skipped, errors);
            OnCompleted(report);
            return report;
        }

        // ----- Steps every supplier has to implement -----

        /// <summary>
        /// Splits the file into rows of "field name - text value".
        /// </summary>
        protected abstract IReadOnlyList<IReadOnlyDictionary<string, string>> ReadRows(string content);

        /// <summary>
        /// Supplier's field names and formats into our product.
        /// </summary>
        protected abstract ImportedProduct Map(IReadOnlyDictionary<string, string> row);

        // ----- Steps with a default implementation, can be overridden -----

        /// <returns>Error message or null when product is valid</returns>
        protected virtual string Validate(ImportedProduct product)
        {
            if (string.IsNullOrWhiteSpace(product.Sku)) return "SKU is missing";
            if (string.IsNullOrWhiteSpace(product.Name)) return $"{product.Sku}: name is missing";
            if (product.NetPricePln <= 0) return $"{product.Sku}: price has to be positive";
            if (product.Stock < 0) return $"{product.Sku}: stock cannot be negative";
            return null;
        }

        // ----- Hooks - do nothing by default -----

        protected virtual ImportedProduct Adjust(ImportedProduct product) => product;
        protected virtual bool ShouldSkip(ImportedProduct product) => false;
        protected virtual void OnCompleted(ImportReport report) { }

        protected static string Field(IReadOnlyDictionary<string, string> row, string name) =>
            row.TryGetValue(name, out var value) ? value?.Trim() : throw new KeyNotFoundException($"Field '{name}' is missing");

    }
}
