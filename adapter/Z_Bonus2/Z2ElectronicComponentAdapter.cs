using adapter.Models;
using adapter.Models.WarehouseZ2;

namespace adapter.Z_Bonus2
{

    /// <summary>
    /// Z1 Warehouse adapter
    /// </summary>
    public partial class ElectronicComponentAdapter : IElectronicComponent
    {

        private void AdaptZ2Component(Z2ElectronicComponent component)
        {
            Id = component.Id.ToString();
            Warehouse = WarehouseType.Z2;
            PN = component.PN;
            MPN = component.MPN;
            if (component.Details != null)
            {
                DateCode = component.Details.DateCode;
                Lot = component.Details.Lot;
            }
        }

    }

}
