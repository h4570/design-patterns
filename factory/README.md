# Factory pattern

In this example, factory pattern was used to calculate discount for different types of customer.
Each customer have own *DiscountPercentage* which is used in CustomerFactory class method.
Please look on **CustomerController.cs** to get better view of this implementation.

## Factory vs Abstract factory

Unlike abstract factory, its main purpose is not to create objects or another factories.  
Factory's goal is to keep the business logic, so it has to be abstract class, not an interface.  
The sample factory has two methods.  
1 - createProduct() method - Creating objects  
2 - doSomething() method - Uses createProduct() and does some operations on it.  
An example implementation of factory may modify createProduct(), but doSomething() remains unchanged.  
Customer will be used mostly doSomething() method.

### Client code
```csharp
[HttpGet]
public IEnumerable<Tuple<ICustomer, decimal>> CreateCustomersAndCalcDiscounts()
{
    var result = new List<Tuple<ICustomer, decimal>>();

    decimal applePrice = 10;

    var veteran = _warVeteransFactory.Create();
    var student = _studentsFactory.Create();

    var veteranDiscount = _warVeteransFactory.AddDiscount(veteran, applePrice);
    var studentDiscount = _studentsFactory.AddDiscount(student, applePrice);

    result.Add(Tuple.Create(veteran, veteranDiscount));
    result.Add(Tuple.Create(student, studentDiscount));

    return result;
}
```

### Factory
```csharp
public abstract class CustomerFactory
{
    public abstract ICustomer Create();
    public abstract IEnumerable<ICustomer> GetAll();

    public decimal AddDiscount(ICustomer customer, decimal price)
    {
        var disc = 100 - customer.DiscountPercentage;
        return price * (disc / (decimal)100);
    }
}
```

### Result (`/customer`)
```json
[
  { "item1": { "id": 2, "fullName": "Carl Johnson", "discountPercentage": 40, "type": 2 }, "item2": 6.0 },
  { "item1": { "id": 1, "fullName": "John Doe", "discountPercentage": 10, "type": 1 }, "item2": 9.0 }
]
```
