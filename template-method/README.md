# Template method pattern

In this example template method pattern was used to **import products from wholesalers** into an online shop.  
Every supplier sends a different file: a Polish wholesaler sends CSV from Excel (semicolons, decimal comma), a European distributor sends JSON with prices in EUR, and an electronics wholesaler sends an XML feed.  
The import itself is always the same: *read rows → map → adjust → validate → skip? → save → report*. That algorithm is written once, in `SupplierImport.Run()`,  
and every supplier overrides only the steps which are different for it.  
To test this example, run "**/import/demo**" endpoint (imports sample files of all suppliers), or send your own file:  
`GET /import/suppliers` (with sample files), `POST /import/{supplier}` with the raw file as `text/plain`, `GET /import/products`.  
Please look on **SupplierImport.cs** and **Suppliers.cs** to get better view of this implementation.

## Participants

- `SupplierImport` - **abstract class**, `Run()` is the **template method** (not virtual - nobody can change the order of steps)
- `ReadRows`, `Map` - **abstract steps**, every supplier has to implement them
- `Validate` - step with a **default implementation**, can be extended
- `Adjust`, `ShouldSkip`, `OnCompleted` - **hooks**, do nothing by default
- `KowalskiCsvImport`, `TechDistriJsonImport`, `ElektroXmlImport` - **concrete classes**

| Supplier | ReadRows | Map | Adjust | Validate | ShouldSkip |
|---|---|---|---|---|---|
| Kowalski (CSV) | semicolons | Polish decimal comma | - | default | **products with 0 stock** |
| TechDistri (JSON) | JsonDocument | EUR price | **EUR → PLN, "TD-" prefix** | default | - |
| Elektro Hurt (XML) | XDocument | - | - | **default + EAN required** | - |

### Template method
```csharp
public ImportReport Run(string content)
{
    var rows = ReadRows(content);
    var errors = new List<string>();
    int imported = 0, skipped = 0;

    for (var i = 0; i < rows.Count; i++)
    {
        try
        {
            var product = Adjust(Map(rows[i]));
            var error = Validate(product);
            if (error != null)
            {
                errors.Add($"Row {i + 1}: {error}");
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
            errors.Add($"Row {i + 1}: {ex.Message}");
        }
    }

    var report = new ImportReport(Supplier, rows.Count, imported, skipped, errors);
    OnCompleted(report);
    return report;
}

protected abstract IReadOnlyList<IReadOnlyDictionary<string, string>> ReadRows(string content);
protected abstract ImportedProduct Map(IReadOnlyDictionary<string, string> row);
protected virtual string Validate(ImportedProduct product) { /* SKU, name, price > 0, stock >= 0 */ }
protected virtual ImportedProduct Adjust(ImportedProduct product) => product;
protected virtual bool ShouldSkip(ImportedProduct product) => false;
protected virtual void OnCompleted(ImportReport report) { }
```

### Concrete class
```csharp
public class TechDistriJsonImport : SupplierImport
{
    protected override IReadOnlyList<IReadOnlyDictionary<string, string>> ReadRows(string content) { /* JsonDocument */ }

    protected override ImportedProduct Map(IReadOnlyDictionary<string, string> row) { /* sku, title, price.amount, qty */ }

    // Hook - prices in EUR and SKUs which collide with other suppliers
    protected override ImportedProduct Adjust(ImportedProduct product) => product with
    {
        Sku = $"TD-{product.Sku}",
        NetPricePln = Math.Round(product.NetPricePln * EurToPln, 2)
    };
}
```

### Client code
```csharp
var reports = _imports.Values.Select(c => c.Run(c.SampleFile)).ToList();
```

### Result (`/import/demo`)
```json
[
  { "supplier": "Hurtownia Kowalski (CSV)", "rows": 5, "imported": 2, "skipped": 1,
    "errors": [ "Row 4: SKU is missing", "Row 5: 'abc' is not a price" ] },
  { "supplier": "TechDistri EU (JSON)", "rows": 3, "imported": 2, "skipped": 0,
    "errors": [ "Row 3: TD-1003: price has to be positive" ] },
  { "supplier": "Elektro Hurt (XML)", "rows": 2, "imported": 1, "skipped": 0,
    "errors": [ "Row 2: EH-502: EAN is required" ] }
]
```
| SKU | Name | Net price PLN | Stock |
|---|---|---|---|
| EH-501 | Power bank 20000 mAh | 79.00 | 60 |
| KW-1001 | Kabel HDMI 2.1 2m | 34.99 | 120 |
| KW-1003 | Mysz bezprzewodowa | 59.90 | 45 |
| TD-1001 | USB-C Hub 7in1 | 107.07 | 300 |
| TD-1002 | Webcam Full HD | 167.70 | 80 |

`KW-1002` (charger with 0 in stock) was skipped by the hook, not reported as an error.

## Template method vs Strategy

Template method uses **inheritance** - the skeleton is in the base class, subclasses fill the gaps. Strategy uses **composition** - the whole algorithm is replaced.  
In .NET you meet template methods all the time: `BackgroundService.ExecuteAsync`, `Controller.OnActionExecuting`, `DbContext.OnModelCreating`, `Stream.Read` overrides.
