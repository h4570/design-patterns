using System;

namespace adapter.Models.WarehouseZ1
{
    public class Z1ElectronicComponent : IWarehouseElectronicComponent
    {
        public Guid Id { get; set; }
        public string Barcode { get; set; }
        public string MPN { get; set; }
    }
}
