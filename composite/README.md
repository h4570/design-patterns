# Composite pattern

In this example composite pattern was used to pack an e-commerce order into **nested shipping boxes**.  
A box can contain products and other boxes (which can contain further boxes...), but both are hidden behind one interface - `IPackageItem`.  
Thanks to that, `ShippingCostCalculator` can price a single monitor or a carton full of boxes with exactly the same code.  
To test this example, run "**/package**" (whole order) and "**/package/single**" (single product) endpoints.  
Please look on **Box.cs** and **PackageController.cs** to get better view of this implementation.

## Participants

- `IPackageItem` - **component**, common interface (price, weight, products count, summary)
- `Product` - **leaf**, single product with own price and weight
- `Box` - **composite**, sums its children recursively and adds own packaging cost/weight
- `ShippingCostCalculator` - **client**, knows only `IPackageItem`

### Composite implementation
```csharp
public decimal GetPrice() => _packagingCost + _items.Sum(c => c.GetPrice());

public decimal GetWeightKg() => _packagingWeightKg + _items.Sum(c => c.GetWeightKg());

public int GetProductsCount() => _items.Sum(c => c.GetProductsCount());
```

### Client code
```csharp
var cablesBag = new Box("Antistatic bag", 0.50m, 0.01m)
    .Add(new Product("USB-C cable", 39.99m, 0.05m))
    .Add(new Product("HDMI cable", 29.99m, 0.08m));

var accessoriesBox = new Box("Small box", 2.00m, 0.15m)
    .Add(new Product("Wireless mouse", 149.00m, 0.09m))
    .Add(new Product("Mechanical keyboard", 459.00m, 1.10m))
    .Add(cablesBag);

// ...

var shippingCost = _shippingCostCalculator.Calculate(package); // works for Box and Product
```

### Result (shortened)
```
Shipping carton        6391.48 PLN   5.63 kg   7 products
├── Laptop box         5504.00 PLN   2.05 kg
│   ├── Laptop 14"
│   └── Laptop charger
├── Small box           680.48 PLN   1.48 kg
│   ├── Wireless mouse
│   ├── Mechanical keyboard
│   └── Antistatic bag   70.48 PLN   0.14 kg
│       ├── USB-C cable
│       └── HDMI cable
└── Desk lamp           199.00 PLN   1.20 kg

Shipping cost: 85.90
```
