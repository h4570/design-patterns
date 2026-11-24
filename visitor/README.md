# Visitor pattern

In this example visitor pattern was used for **summary of a shopping cart** in an online store, where the cart contains very different things:
a physical product, an ebook, a gift card and an installation service at home.  
Every operation treats each of them differently:
- **VAT** - 23% for electronics and services, 5% for books and ebooks, gift card is *not subject to VAT* yet (VAT is paid when it is used),
- **delivery** - only physical products go into the parcel, ebook is a download link, gift card a code by e-mail, service a technician visit,
- **accounting** - goods, digital goods and services are revenue, gift card is a **liability** (we still owe the goods).

Instead of putting `CalculateVat()`, `PlanDelivery()` and `ToAccountingLine()` into every item class (and a new method for every new report),
each operation is a separate visitor with one `Visit` method per item type. Items only have `Accept(visitor)`.  
To test this example, run "**/cart/demo**" endpoint, or `POST /cart/summary` with your own items:  
`[{"type":"ebook","name":"DDD","grossPrice":79,"format":"PDF"},{"type":"physical","name":"Mug","grossPrice":30,"quantity":2,"weightKg":0.4,"vatPercent":23}]`.  
Please look on **CartItems.cs** and **CartVisitors.cs** to get better view of this implementation.

## Participants

- `ICartVisitor<T>` - **visitor**, generic, so every visitor returns its own result type
- `VatCalculator`, `DeliveryPlanner`, `AccountingExport` - **concrete visitors**, one operation each
- `ICartItem` - **element**, only `Accept<T>(visitor)`
- `PhysicalProduct`, `Ebook`, `GiftCard`, `InstallationService` - **concrete elements**, simple records
- `CartController` - **client**, runs three visitors over the same items

### Elements and double dispatch
```csharp
public interface ICartVisitor<out T>
{
    T Visit(PhysicalProduct item);
    T Visit(Ebook item);
    T Visit(GiftCard item);
    T Visit(InstallationService item);
}

public record Ebook(string Name, decimal GrossPrice, string Format) : ICartItem
{
    // "this" is Ebook here, so the compiler picks Visit(Ebook) - no type checks or casts needed
    public T Accept<T>(ICartVisitor<T> visitor) => visitor.Visit(this);
}
```

### Concrete visitor
```csharp
public class VatCalculator : ICartVisitor<VatLine>
{
    public VatLine Visit(PhysicalProduct item) => FromGross(item.Name, item.GrossPrice * item.Quantity, item.VatPercent);
    public VatLine Visit(Ebook item) => FromGross(item.Name, item.GrossPrice, 5);
    public VatLine Visit(GiftCard item) => new(item.Name, "NP", item.Value, item.Value, 0m);
    public VatLine Visit(InstallationService item) => FromGross(item.Name, item.GrossPrice, 23);
}
```

### Client code
```csharp
// Three different operations on the same items - each one is a visitor
var vat = items.Select(c => c.Accept(new VatCalculator())).ToList();
var deliveries = items.Select(c => c.Accept(new DeliveryPlanner())).ToList();
var accounting = items.Select(c => c.Accept(new AccountingExport())).ToList();
```

### Result (`/cart/demo`)
Total: **5636.88 PLN**, shipping: *Parcel 2.5 kg - free shipping*

| Rate | Net | VAT |
|---|---|---|
| 23% | 4266.65 | 981.33 |
| 5% | 179.91 | 8.99 |
| NP | 200.00 | 0.00 |

| Item | Delivery | Accounting |
|---|---|---|
| Laptop 14" | Parcel, 1.4 kg | 700 Revenue - goods; net 4064.23; VAT 23% 934.77 |
| USB-C cable 2m (x2) | Parcel, 0.2 kg | 700 Revenue - goods; net 81.28; VAT 23% 18.70 |
| Book: Design Patterns (paper) | Parcel, 0.9 kg | 700 Revenue - goods; net 122.86; VAT 5% 6.14 |
| Clean Architecture | Download link (EPUB) by e-mail | 701 Revenue - digital goods; net 57.05; VAT 5% 2.85 |
| Gift card | Code sent to piotr@example.com | 845 Liability - gift cards; net 200.00; VAT NP 0.00 |
| Laptop setup at home | Technician visit in Krakow | 702 Revenue - services; net 121.14; VAT 23% 27.86 |

## When to use it

Visitor makes **adding operations** easy (new class, nothing else changes) and **adding element types** hard (every visitor needs a new `Visit`).  
So it fits when types are stable and operations keep coming - like here, where accounting, marketing or warehouse keep asking for new reports.  
Since C# 8 a `switch` expression with type patterns is a lighter alternative, but the compiler will not tell you which `switch` forgot about a new type - with a visitor it will.  
Roslyn (`CSharpSyntaxVisitor`) and `ExpressionVisitor` for LINQ expression trees are visitors you can find in .NET itself.
