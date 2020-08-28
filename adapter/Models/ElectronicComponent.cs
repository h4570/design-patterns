namespace adapter.Models
{

    public interface IElectronicComponent
    {
        public string Id { get; }
        public WarehouseType Warehouse { get; }
        public string PN { get; }
        public string MPN { get; }
        public string DateCode { get; }
        public string Lot { get; }
    }

    public class ElectronicComponent : IElectronicComponent
    {
        public string Id { get; set; }
        public WarehouseType Warehouse { get; set; }
        public string PN { get; set; }
        public string MPN { get; set; }
        public string DateCode { get; set; }
        public string Lot { get; set; }
    }

}
