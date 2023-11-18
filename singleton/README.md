# Singleton pattern

In this example, Singleton was used to implement thread safe file *LoggerService* in Web API.  
In second screenshot, you can see run of stress test 100 GET requests which are using LoggerService in 6 seconds  
Please look on **LoggerTestController.cs** to get better view of this implementation.

### Client code
![Client code](https://raw.githubusercontent.com/h4570/design-patterns/master/singleton/pic-1.png)

![Stress test](https://raw.githubusercontent.com/h4570/design-patterns/master/singleton/pic-2.png)

![Output file](https://raw.githubusercontent.com/h4570/design-patterns/master/singleton/pic-3.png)
