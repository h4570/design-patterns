# Prototype pattern

Prototype is a simple design pattern. Which can be implemented in .NET very easily.  
The big game changer is native *MemberwiseClone()* method which is creating shadow copy of  
current object. In other programming languages we would need also, second construtor  
and property copying in *Clone()* method.  
Please look on **DevicesController.cs** to get better view of this implementation.

### Client code
![Client code](https://raw.githubusercontent.com/h4570/design-patterns/master/prototype/pic-1.png)

![Client code](https://raw.githubusercontent.com/h4570/design-patterns/master/prototype/pic-2.png)
