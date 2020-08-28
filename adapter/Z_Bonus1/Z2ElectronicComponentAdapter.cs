using adapter.Models;
using adapter.Models.WarehouseZ2;

namespace adapter.Z_Bonus1
{

    public class Z2ElectronicComponentAdapter : IElectronicComponent
    {

        public string Id => _originalComponent.Id.ToString();
        public WarehouseType Warehouse => WarehouseType.Z2;
        public string PN => _originalComponent.PN;
        public string MPN => _originalComponent.MPN;
        public string DateCode { get; private set; }
        public string Lot { get; private set; }

        private readonly Z2ElectronicComponent _originalComponent;

        public Z2ElectronicComponentAdapter(Z2ElectronicComponent originalComponent)
        {
            _originalComponent = originalComponent;
            if (_originalComponent.Details != null)
            {
                DateCode = _originalComponent.Details.DateCode;
                Lot = _originalComponent.Details.Lot;
            }
        }

    }

}
