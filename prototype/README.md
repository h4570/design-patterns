# Prototype pattern

Prototype is a simple design pattern. Which can be implemented in .NET very easily.  
The big game changer is native *MemberwiseClone()* method which is creating shadow copy of  
current object. In other programming languages we would need also, second construtor  
and property copying in *Clone()* method.  
Please look on **DevicesController.cs** to get better view of this implementation.

### Client code
```csharp
[HttpGet]
public ActionResult<IEnumerable<IDevice>> Get()
{
    var smartphone = new Smartphone() { Id = 1, Name = "Super phone" };
    _deviceProtoRegistry.Add(smartphone);

    var copy = _deviceProtoRegistry.GetCopyById(1);
    copy.Id = 2;

    return Ok(new List<IDevice>() { smartphone, copy });
}
```

### Prototype registry
```csharp
public class DevicePrototypeRegistry
{
    private readonly List<IDevice> _items;

    public DevicePrototypeRegistry()
    {
        _items = new List<IDevice>();
    }

    public void Add(IDevice device) { _items.Add(device); }

    public IDevice GetCopyById(int id)
    {
        var item = _items.SingleOrDefault(c => c.Id.Equals(id));
        if (item != null) return item.Clone();
        else throw new System.Exception("Provided device id was not found");
    }
}
```
