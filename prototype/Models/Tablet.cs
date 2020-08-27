namespace prototype.Models
{
    public class Tablet : IDevice
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IDevice Clone() { return (IDevice)MemberwiseClone(); }
    }
}
