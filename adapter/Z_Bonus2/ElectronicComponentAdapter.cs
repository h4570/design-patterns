using adapter.Models;
using adapter.Models.WarehouseZ1;
using adapter.Models.WarehouseZ2;
using System;

namespace adapter.Z_Bonus2
{

    public partial class ElectronicComponentAdapter : IElectronicComponent
    {

        public string Id { get; private set; }
        public WarehouseType Warehouse { get; private set; }
        public string PN { get; private set; }
        public string MPN { get; private set; }
        public string DateCode { get; private set; }
        public string Lot { get; private set; }

        public ElectronicComponentAdapter(IWarehouseElectronicComponent originalComponent)
        {
            if (originalComponent is Z1ElectronicComponent z1Component)
                AdaptZ1Component(z1Component);
            else if (originalComponent is Z2ElectronicComponent z2Component)
                AdaptZ2Component(z2Component);
            else throw new Exception("Adapter currently supports Z1 and Z2 components only.");
        }

    }

}
