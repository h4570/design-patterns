# Command pattern

In this example command pattern was used for **bulk changes in an e-commerce product catalog** (admin panel) with **undo / redo**.  
Every change - new price, category discount, publishing a product - is an object implementing `ICatalogCommand` with `Execute()` and `Undo()`.  
`CommandHistory` executes commands and keeps them on stacks, so any change can be undone and redone, and the history shows who changed what.  
Few commands can be grouped into one `MacroCommand` - "Black Friday" discounts two categories and publishes a hidden bundle, and is undone as a single step.  
To test this example, run "**/catalog/demo**" endpoint (whole scenario on a separate catalog), or play with the shared catalog:  
`POST /catalog/price`, `POST /catalog/discount`, `POST /catalog/black-friday`, `POST /catalog/undo`, `POST /catalog/redo`, `GET /catalog/history`.  
Please look on **CatalogCommands.cs** and **CommandHistory.cs** to get better view of this implementation.

## Participants

- `ICatalogCommand` - **command**, request turned into an object
- `ChangePriceCommand`, `CategoryDiscountCommand`, `PublishProductCommand` - **concrete commands**, remember what is needed to undo
- `MacroCommand` - group of commands executed as one, all or nothing
- `CommandHistory` - **invoker**, executes commands, keeps undo / redo stacks, knows nothing about products
- `ProductCatalog` - **receiver**, does the real work, knows nothing about commands
- `CatalogController` - **client**, creates commands and passes them to the invoker

### Command implementation
```csharp
public class CategoryDiscountCommand : ICatalogCommand
{
    public string Description => $"{_percent}% discount on {_category}";

    public void Execute()
    {
        _oldPrices.Clear();
        foreach (var product in _catalog.InCategory(_category))
        {
            _oldPrices[product.Sku] = product.Price;      // remembered for Undo()
            var discounted = Math.Floor(product.Price * (100 - _percent) / 100) - 0.01m;
            _catalog.SetPrice(product.Sku, discounted);
        }
    }

    public void Undo()
    {
        foreach (var (sku, price) in _oldPrices)
            _catalog.SetPrice(sku, price);
    }
}
```

### Invoker
```csharp
public void Execute(ICatalogCommand command)
{
    lock (_lock)
    {
        command.Execute();      // when it throws, nothing lands in history
        _done.Push((command, DateTime.Now));
        _undone.Clear();        // new change makes old "redo" path invalid
    }
}

public string Undo()
{
    lock (_lock)
    {
        if (_done.Count == 0) return null;
        var entry = _done.Pop();
        entry.Command.Undo();
        _undone.Push(entry);
        return entry.Command.Description;
    }
}
```

### Client code
```csharp
history.Execute(new ChangePriceCommand(catalog, "MON-27", 1799m));
history.Execute(new MacroCommand("Black Friday",
    new CategoryDiscountCommand(catalog, "Electronics", 20),
    new CategoryDiscountCommand(catalog, "Accessories", 30),
    new PublishProductCommand(catalog, "BF-BUNDLE")));

history.Undo();   // whole Black Friday at once
history.Redo();
```

### Result (`/catalog/demo`)
| Step | LAP-14 | MON-27 | KB-001 | MS-002 | BF-BUNDLE |
|---|---|---|---|---|---|
| Start | 4999 | 1899 | 459 | 149 | *hidden* |
| Execute: change price of MON-27 | 4999 | **1799** | 459 | 149 | *hidden* |
| Execute: Black Friday macro | **3998.99** | **1438.99** | **320.99** | **103.99** | **4799** |
| Undo: Black Friday | 4999 | 1799 | 459 | 149 | *hidden* |
| Redo: Black Friday | 3998.99 | 1438.99 | 320.99 | 103.99 | 4799 |
| Undo: Black Friday | 4999 | 1799 | 459 | 149 | *hidden* |
| Undo: Change price of MON-27 to 1799.00 | 4999 | 1899 | 459 | 149 | *hidden* |

## What else commands give you

Because a command is just an object, besides undo it can be serialized and put on a queue (executed later by a background worker),  
logged as an audit trail, or retried. MediatR's `IRequest` + handler in many .NET projects is the same idea, without undo.
