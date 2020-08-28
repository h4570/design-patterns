using System;

namespace adapter.Models.WarehouseZ2
{
    public class Z2ElectronicComponent : IWarehouseElectronicComponent
    {
        public int Id { get; set; }
        public Z2ElectronicComponentDetails Details { get; set; }
        public string PN { get; set; }
        public string MPN { get; set; }
    }
}
