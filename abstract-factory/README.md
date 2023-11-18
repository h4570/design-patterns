# Abstract factory pattern

In this example abstract factory pattern was used to use different cooking logic for various croissants.  
As you can see, there are two implementations of CroissantFactory: *Chocolate* and *Sweet*.  
Base logic which is used by both implementations is created in Croissant class.
To test this example, you can run "**/croissant/create**" endpoint and compare results after small change in endpoint logic.

## Abstract factory vs Factory

Unlike factory, its main purpose is to create objects or another factories.  
I described core difference between factory and abstract factory in factory/README.md

### Client code
![Client code](https://raw.githubusercontent.com/h4570/design-patterns/abstract-factory/pic-1.png)

![Client code](https://raw.githubusercontent.com/h4570/design-patterns/abstract-factory/pic-2.png)
