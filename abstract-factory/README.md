# Abstract factory pattern

In this example abstract factory pattern was used to use different cooking logic for various croissants.  
As you can see, there are two implementations of CroissantFactory: *Chocolate* and *Sweet*.  
Base logic which is used by both implementations is created in Croissant class.
To test this example, you can run "**/croissant/create**" endpoint and compare results after small change in endpoint logic.

## Abstract factory vs Factory

Unlike factory, its main purpose is to create objects or another factories.  
I described core difference between factory and abstract factory in factory/README.md

### Client code
```csharp
[HttpGet("create")]
public ActionResult<Croissant> CreateChocolateCroissant()
{
    var customization = new CroissantCustomization()
    {
        Chocolate = CroissantChocolateAmount.DoubleChocolate
    };
    var croissant = CroissantFactory
        .GetFactory(CroissantType.Chocolate)
        .GetCroissant(_machineService, customization);
    croissant.Make();
    return Ok(croissant);
}
```

### Abstract factory
```csharp
public abstract class CroissantFactory
{
    public abstract Croissant GetCroissant(MachineService service, CroissantCustomization customization);

    public static CroissantFactory GetFactory(CroissantType type)
    {
        return type switch
        {
            CroissantType.Chocolate => new ChocolateCroissantFactory(),
            CroissantType.Sweet => new SweetCroissantFactory(),
            _ => throw new NotImplementedException()
        };
    }
}
```

### Files structure
```
abstract-factory
├── Controllers
│   └── CroissantController.cs
├── CroissantAbstractFactory
│   ├── ChocolateCroissant.cs
│   ├── ChocolateCroissantFactory.cs
│   ├── Croissant.cs
│   ├── CroissantCustomization.cs
│   ├── CroissantFactory.cs
│   ├── SweetCroissant.cs
│   └── SweetCroissantFactory.cs
└── Services
    └── MachineService.cs
```
