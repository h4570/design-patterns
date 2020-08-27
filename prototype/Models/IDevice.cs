using System;

namespace prototype.Models
{
    public interface IDevice
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IDevice Clone();
    }
}
