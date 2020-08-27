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
![Client code](http://apgcglz.cluster028.hosting.ovh.net/design-patterns/factory-1.png)

![Client code](http://apgcglz.cluster028.hosting.ovh.net/design-patterns/factory-2.png)

![Client code](http://apgcglz.cluster028.hosting.ovh.net/design-patterns/factory-3.png)
