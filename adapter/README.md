# Adapter pattern

In this example adapter pattern was used for conversion of two different electronic component objects into final one.  
**Extension method which you can see below the client code** gives the client huge flexibility with using and creating new adapters.  
Current implementation is more professional, but if you look for simpler way, feel free to check **bonus folders** where  
you can see **two basic implementations of adapter design - simple way and partial class way**.

### Client code
```csharp
[HttpGet]
public ActionResult<IEnumerable<IElectronicComponent>> Get()
{
    var result = new List<IElectronicComponent>();

    var z1Adapter = new Z1ElectronicComponentAdapter();
    var z1Components = _z1ComponentsRepo.GetAll();
    result.AddRange(z1Components.ToElectronicComponents(z1Adapter));

    var z2Adapter = new Z2ElectronicComponentAdapter();
    var z2Components = _z2ComponentsRepo.GetAll();
    result.AddRange(z2Components.ToElectronicComponents(z2Adapter));

    return Ok(result);
}
```

### Extension method for objects adapting
```csharp
public static class IWarehouseElectronicComponentExtensions
{
    public static IEnumerable<IElectronicComponent> ToElectronicComponents<T>(
            this IEnumerable<T> warehouseComponents,
            IElectronicComponentAdapter<T> adapter
        ) where T : IWarehouseElectronicComponent
    {
        foreach (var component in warehouseComponents)
            yield return adapter.Adapt(component);
    }
}
```

### What are we adapting
Two warehouses keep the same components in completely different shapes...
```csharp
public class Z1ElectronicComponent : IWarehouseElectronicComponent
{
    public Guid Id { get; set; }
    public string Barcode { get; set; }
    public string MPN { get; set; }
}

public class Z2ElectronicComponent : IWarehouseElectronicComponent
{
    public int Id { get; set; }
    public Z2ElectronicComponentDetails Details { get; set; }
    public string PN { get; set; }
    public string MPN { get; set; }
}
```
...and both of them have to become this:
```csharp
public interface IElectronicComponent
{
    public string Id { get; }
    public WarehouseType Warehouse { get; }
    public string PN { get; }
    public string MPN { get; }
    public string DateCode { get; }
    public string Lot { get; }
}
```

### Example adapter
```csharp
public class Z1ElectronicComponentAdapter : IElectronicComponentAdapter<Z1ElectronicComponent>
{
    public IElectronicComponent Adapt(Z1ElectronicComponent component)
    {
        var result = new Models.ElectronicComponent();
        if (component.Id != Guid.Empty)
            result.Id = component.Id.ToString();
        result.Warehouse = WarehouseType.Z1;
        result.MPN = component.MPN;
        var (pn, lot, dateCode) = GetBarcodeVariables(component);
        result.PN = pn;
        result.Lot = lot;
        result.DateCode = dateCode;
        return result;
    }

    private (string pn, string lot, string dateCode) GetBarcodeVariables(Z1ElectronicComponent component)
    {
        if (component.Barcode != null && component.Barcode.Contains(';'))
        {
            var splittedBarcode = component.Barcode.Split(';');
            var pn = RemovePrefixIfExists(GetSplittedBarcodePart(splittedBarcode, 0));
            var lot = RemovePrefixIfExists(GetSplittedBarcodePart(splittedBarcode, 1));
            var dateCode = RemovePrefixIfExists(GetSplittedBarcodePart(splittedBarcode, 2));
            return (pn, lot, dateCode);
        }
        return (null, null, null);
    }

    // ...
}
```

### Folders structure
```
adapter
├── Adapters
│   └── ElectronicComponent
│       ├── IElectronicComponentAdapter.cs
│       ├── Z1ElectronicComponentAdapter.cs
│       └── Z2ElectronicComponentAdapter.cs
├── Controllers
│   └── ComponentsController.cs
├── Extensions
│   └── IWarehouseElectronicComponentExtensions.cs
├── Models
│   ├── WarehouseZ1
│   │   └── Z1ElectronicComponent.cs
│   ├── WarehouseZ2
│   │   ├── Z2ElectronicComponent.cs
│   │   └── Z2ElectronicComponentDetails.cs
│   ├── ElectronicComponent.cs
│   ├── IWarehouseElectronicComponent.cs
│   └── WarehouseType.cs
├── Repositories
├── Z_Bonus1
└── Z_Bonus2
```
