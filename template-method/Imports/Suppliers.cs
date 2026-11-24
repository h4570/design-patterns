using System.Globalization;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;

namespace template_method.Imports
{
    /// <summary>
    /// Polish wholesaler - CSV exported from Excel: semicolons, decimal comma.
    /// They send products with zero stock too, we do not want them in the shop.
    /// </summary>
    public class KowalskiCsvImport : SupplierImport
    {

        private static readonly CultureInfo Polish = CultureInfo.GetCultureInfo("pl-PL");

        public KowalskiCsvImport(ProductRepository repository) : base(repository) { }

        public override string Key => "kowalski-csv";
        public override string Supplier => "Hurtownia Kowalski (CSV)";

        public override string SampleFile =>
            """
            kod;nazwa;cena_netto;stan
            KW-1001;Kabel HDMI 2.1 2m;34,99;120
            KW-1002;Ladowarka USB-C 65W;89,00;0
            KW-1003;Mysz bezprzewodowa;59,90;45
            ;Bez kodu;10,00;5
            KW-1005;Podkladka pod mysz;abc;10
            """;

        protected override IReadOnlyList<IReadOnlyDictionary<string, string>> ReadRows(string content)
        {
            var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var header = lines[0].Split(';');
            return lines.Skip(1)
                .Select(line => (IReadOnlyDictionary<string, string>)header
                    .Zip(line.Split(';'))
                    .ToDictionary(c => c.First, c => c.Second))
                .ToList();
        }

        protected override ImportedProduct Map(IReadOnlyDictionary<string, string> row) => new(
            Field(row, "kod"),
            Field(row, "nazwa"),
            decimal.TryParse(Field(row, "cena_netto"), NumberStyles.Number, Polish, out var price)
                ? price
                : throw new FormatException($"'{Field(row, "cena_netto")}' is not a price"),
            int.Parse(Field(row, "stan")));

        protected override bool ShouldSkip(ImportedProduct product) => product.Stock == 0;

    }

    /// <summary>
    /// European distributor - JSON with prices in EUR. Their SKUs collide with others, so we add a prefix.
    /// </summary>
    public class TechDistriJsonImport : SupplierImport
    {

        public const decimal EurToPln = 4.30m;

        public TechDistriJsonImport(ProductRepository repository) : base(repository) { }

        public override string Key => "techdistri-json";
        public override string Supplier => "TechDistri EU (JSON)";

        public override string SampleFile =>
            """
            [
              { "sku": "1001", "title": "USB-C Hub 7in1", "price": { "amount": 24.90, "currency": "EUR" }, "qty": 300 },
              { "sku": "1002", "title": "Webcam Full HD", "price": { "amount": 39.00, "currency": "EUR" }, "qty": 80 },
              { "sku": "1003", "title": "Laptop stand", "price": { "amount": -5, "currency": "EUR" }, "qty": 10 }
            ]
            """;

        protected override IReadOnlyList<IReadOnlyDictionary<string, string>> ReadRows(string content)
        {
            try
            {
                using var document = JsonDocument.Parse(content);
                return document.RootElement.EnumerateArray()
                    .Select(item => (IReadOnlyDictionary<string, string>)new Dictionary<string, string>
                    {
                        ["sku"] = item.GetProperty("sku").GetString(),
                        ["title"] = item.GetProperty("title").GetString(),
                        ["amount"] = item.GetProperty("price").GetProperty("amount").GetRawText(),
                        ["currency"] = item.GetProperty("price").GetProperty("currency").GetString(),
                        ["qty"] = item.GetProperty("qty").GetRawText()
                    })
                    .ToList();
            }
            catch (Exception ex) when (ex is JsonException or KeyNotFoundException or InvalidOperationException)
            {
                throw new FormatException($"Invalid JSON file: {ex.Message}", ex);
            }
        }

        protected override ImportedProduct Map(IReadOnlyDictionary<string, string> row)
        {
            if (Field(row, "currency") != "EUR")
                throw new FormatException($"Currency {Field(row, "currency")} is not supported");
            return new(
                Field(row, "sku"),
                Field(row, "title"),
                decimal.Parse(Field(row, "amount"), CultureInfo.InvariantCulture),
                int.Parse(Field(row, "qty")));
        }

        protected override ImportedProduct Adjust(ImportedProduct product) => product with
        {
            Sku = $"TD-{product.Sku}",
            NetPricePln = Math.Round(product.NetPricePln * EurToPln, 2)
        };

    }

    /// <summary>
    /// Electronics wholesaler - XML feed. Uses default validation, but has an extra rule: EAN is required.
    /// </summary>
    public class ElektroXmlImport : SupplierImport
    {

        public ElektroXmlImport(ProductRepository repository) : base(repository) { }

        public override string Key => "elektro-xml";
        public override string Supplier => "Elektro Hurt (XML)";

        public override string SampleFile =>
            """
            <products>
              <product id="EH-501" ean="5901234123457"><name>Power bank 20000 mAh</name><netPrice>79.00</netPrice><available>60</available></product>
              <product id="EH-502" ean=""><name>Smartwatch strap</name><netPrice>19.00</netPrice><available>200</available></product>
            </products>
            """;

        protected override IReadOnlyList<IReadOnlyDictionary<string, string>> ReadRows(string content)
        {
            try
            {
                return XDocument.Parse(content).Root.Elements("product")
                    .Select(c => (IReadOnlyDictionary<string, string>)new Dictionary<string, string>
                    {
                        ["id"] = (string)c.Attribute("id"),
                        ["ean"] = (string)c.Attribute("ean"),
                        ["name"] = (string)c.Element("name"),
                        ["netPrice"] = (string)c.Element("netPrice"),
                        ["available"] = (string)c.Element("available")
                    })
                    .ToList();
            }
            catch (XmlException ex)
            {
                throw new FormatException($"Invalid XML file: {ex.Message}", ex);
            }
        }

        protected override ImportedProduct Map(IReadOnlyDictionary<string, string> row) => new(
            Field(row, "id"),
            Field(row, "name"),
            decimal.Parse(Field(row, "netPrice"), CultureInfo.InvariantCulture),
            int.Parse(Field(row, "available")),
            Field(row, "ean"));

        // Default rules plus one more
        protected override string Validate(ImportedProduct product) =>
            base.Validate(product) ?? (string.IsNullOrEmpty(product.Ean) ? $"{product.Sku}: EAN is required" : null);

    }
}
