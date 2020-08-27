# Abstract factory pattern

In this example abstract factory pattern was used to manage database provider.  
As you can see, there are two implementations of DbFactory: *PostgreSQL* and *Mongodb*  
To test this example, you can run "**/user/latest**" endpoint and compare results after change of **Constants.CURRENT_DATABASE** variable

## Abstract factory vs Factory

Unlike factory, its main purpose is to create objects or another factories.  
I described core difference between factory and abstract factory in factory/README.md

### Client code
![Client code](http://apgcglz.cluster028.hosting.ovh.net/design-patterns/abstract-factory-1.png)

![Client code](http://apgcglz.cluster028.hosting.ovh.net/design-patterns/abstract-factory-2.png)
