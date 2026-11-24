# Mediator pattern

In this example mediator pattern was used for a **car rental reservation form**, where fields depend on each other:
- driver younger than 25 cannot rent Premium or Van (already selected class is changed to Economy), younger than 21 cannot rent at all,
- Premium car makes full insurance mandatory (checkbox is checked and locked),
- return station is prefilled with the pickup station,
- airport pickup, one-way trip, young driver and a week long rental change the price.

Every field (`FormComponent`) knows **only the mediator**. When it changes, it calls `mediator.Notify(this)` and `RentalFormMediator` decides what happens to other fields.  
Without the mediator, age input would need references to car class select, insurance checkbox and price summary, and car class select to insurance and price - a web of dependencies.  
To test this example, run "**/rental-form/demo**" endpoint, or `POST /rental-form` with your own list of changes, e.g. `[{"field":"driverAge","value":"19"},{"field":"carClass","value":"Economy"}]`.  
Please look on **RentalFormMediator.cs** to get better view of this implementation.

## Participants

- `IFormMediator` - **mediator** interface, one method: `Notify(sender)`
- `RentalFormMediator` - **concrete mediator**, all rules between fields in one place
- `StationPicker`, `DateRangePicker`, `NumberInput`, `CarClassSelect`, `Checkbox`, `PriceSummary` - **colleagues**, reusable and unaware of each other
- `CarRentalForm` - creates the components and connects them through the mediator
- `RentalFormController` - **client**, changes fields like a user would

### Colleague
```csharp
public void Change(string value)
{
    if (!Enabled)
        throw new InvalidOperationException($"Field '{Name}' is disabled: {Hint}");
    Parse(value);
    _mediator?.Notify(this);   // "I changed" - component does not care who reacts
}
```

### Mediator
```csharp
public void Notify(FormComponent sender)
{
    // Most people return the car where they picked it up
    if (sender == _pickup && _return.Station == null)
        _return.Prefill(_pickup.Station);   // Prefill does not notify back - no ping-pong

    if (sender == _carClass)
        _carClass.Hint = null;

    if (sender == _driverAge)
        ApplyAgeRules();

    if (sender == _carClass || sender == _driverAge)
        ApplyInsuranceRules();

    // Every change can affect the price
    RecalculatePrice();
}
```

### Client code
```csharp
var form = new CarRentalForm();
form.Apply(new FieldChange("carClass", "Premium"));
form.Apply(new FieldChange("driverAge", "23"));   // mediator changes carClass and unlocks insurance
```

### Result (`/rental-form/demo`, shortened)
| Change | Reaction of other fields | Total |
|---|---|---|
| pickupStation = Warsaw Chopin Airport | returnStation prefilled with the same station | *Fill in: dates, driverAge, carClass* |
| dates = 2026-12-20/2026-12-27 | - | *Fill in: driverAge, carClass* |
| carClass = Premium | fullInsurance checked and **disabled** - "mandatory for Premium cars" | *Fill in: driverAge* |
| driverAge = 23 | carClass **changed to Economy** - "Premium is available from 25 years", fullInsurance enabled again | 1376.00 |
| fullInsurance = false | - | 1061.00 |
| returnStation = Gdansk Main Station | one-way fee added | 1211.00 |

```
Economy x 7 days: 840.00 PLN
Weekly discount -10%: -84.00 PLN
Young driver fee x 7 days: 245.00 PLN
Airport fee: 60.00 PLN
One-way fee: 150.00 PLN
```

When age is below 21, car class is disabled and trying to change it returns `400 Field 'carClass' is disabled: Drivers have to be at least 21 years old`.

## Mediator vs MediatR

The popular MediatR library takes the name, but it is closer to a command / request dispatcher - sender does not know the handler.  
Classic mediator, like here, coordinates many objects which would otherwise talk to each other directly.  
Other examples: air traffic control tower, chat room server, or `EventAggregator` in desktop MVVM applications.
