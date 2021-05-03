# Abstract factory pattern

In this example abstract factory pattern was used to use different cooking logic for various croissants.  
As you can see, there are two implementations of CroissantFactory: *Chocolate* and *Sweet*.  
Base logic which is used by both implementations is created in Croissant class.
To test this example, you can run "**/croissant/create**" endpoint and compare results after small change in endpoint logic.

## Abstract factory vs Factory

Unlike factory, its main purpose is to create objects or another factories.  
I described core difference between factory and abstract factory in factory/README.md

### Client code
![Client code](http://apgcglz.cluster028.hosting.ovh.net/design-patterns/abstract-factory-1.png)

![Client code](http://apgcglz.cluster028.hosting.ovh.net/design-patterns/abstract-factory-2.png)
