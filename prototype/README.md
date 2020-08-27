# Prototype pattern

Prototype is a simple design pattern. Which can be implemented in .NET very easily.  
The big game changer is natie *MemberwiseClone()* method which is creating shadow copy of  
current object. In other programming languages implementation need also, second construtor  
and property copying in *Clone()* method.  
Please look on **DevicesController.cs** to get better view of this implementation.

### Client code
![Client code](http://apgcglz.cluster028.hosting.ovh.net/design-patterns/prototype-1.png)

![Client code](http://apgcglz.cluster028.hosting.ovh.net/design-patterns/prototype-2.png)
