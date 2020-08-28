# Singleton pattern

In this example, Singleton was used to implement thread safe file *LoggerService* in Web API.  
In second screenshot, you can see run of stress test 100 GET requests which are using LoggerService in 6 seconds  
Please look on **LoggerTestController.cs** to get better view of this implementation.

### Client code
![Client code](http://apgcglz.cluster028.hosting.ovh.net/design-patterns/singleton-1.png)

![Stress test](http://apgcglz.cluster028.hosting.ovh.net/design-patterns/singleton-2.png)

![Output file](http://apgcglz.cluster028.hosting.ovh.net/design-patterns/singleton-3.png)
