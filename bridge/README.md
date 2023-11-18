# Bridge pattern

In this example bridge pattern was used to manage database provider.  
As you can see, there are two implementations of IDatabaseProvider: *PostgreSQL* and *Mongodb*  
To test this example, you can run "**/user/latest**" endpoint and compare results after changing first parameter of `Utilities.GetCurrentDbProvider()` function.

### Client code
![Client code](https://raw.githubusercontent.com/h4570/design-patterns/master/bridge/pic-1.png)

![Client code](https://raw.githubusercontent.com/h4570/design-patterns/master/bridge/pic-2.png)
