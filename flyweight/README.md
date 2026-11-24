# Flyweight pattern

In this example flyweight pattern was used to keep the **seat map of a 40 000 seats stadium** in memory (ticket sales system).  
Every seat has its own position and status, but there are only 3 seat categories (*VIP*, *Premium*, *Standard*), and category data is heavy - description, perks list and SVG icon for the seat map.  
Instead of copying it into every seat, seats share one immutable `SeatCategory` object per category, handed out by `SeatCategoryFactory`.  
To test this example, run "**/stadium/memory**" (with vs without flyweight), "**/stadium/categories**" and "**/stadium/sectors/B/rows/3**" endpoints.  
You can also buy a seat with `POST /stadium/sectors/B/rows/3/seats/2/buy` - only extrinsic state of the seat changes, shared category stays untouched.  
Please look on **SeatCategoryFactory.cs** and **Seat.cs** to get better view of this implementation.

## Participants

- `SeatCategory` - **flyweight**, intrinsic (shared, immutable) state: name, price, color, perks, icon
- `SeatCategoryFactory` - **flyweight factory**, creates each category once and then returns the same instance
- `Seat` - **context**, extrinsic (unique) state: sector, row, number, sold flag + reference to the flyweight
- `NaiveSeat` - the same seat without flyweight, used only for comparison

### Flyweight factory
```csharp
public class SeatCategoryFactory
{
    private readonly ConcurrentDictionary<string, SeatCategory> _categories = new();

    public SeatCategory Get(string name) => _categories.GetOrAdd(name, SeatCategoryCatalog.Load);
}
```

### Client code
```csharp
// Without flyweight - every seat gets its own copy of category data
var naiveSeats = StadiumLayout.Build((sector, row, number, category) =>
    new NaiveSeat(sector, row, number, SeatCategoryCatalog.Load(category)));

// With flyweight - 40 000 seats, 3 category objects
var flyweightSeats = StadiumLayout.Build((sector, row, number, category) =>
    new Seat(sector, row, number, factory.Get(category)));
```

### Result (`/stadium/memory`)
```json
{
  "seats": 40000,
  "withoutFlyweight": { "categoryObjects": 40000, "memoryMb": 57.83 },
  "withFlyweight": { "categoryObjects": 3, "memoryMb": 2.14 },
  "memorySavedPercent": 96.3
}
```

### Result (`/stadium/categories`, shortened)
```json
[
  { "name": "VIP", "price": 899, "seatsSharingThisInstance": 5000, "sold": 1732 },
  { "name": "Premium", "price": 349, "seatsSharingThisInstance": 15000, "sold": 5138 },
  { "name": "Standard", "price": 149, "seatsSharingThisInstance": 20000, "sold": 7111 }
]
```

## When to use it

Flyweight makes sense only when you have **a lot** of objects and most of their state repeats.  
Shared state has to be **immutable** - if one seat could change the price of its category, it would change it for 5000 other seats too.  
In .NET you already use this pattern every day - interned strings and `Encoding.UTF8` are flyweights as well.
