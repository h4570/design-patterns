using adapter.Models;
using adapter.Models.WarehouseZ2;

namespace adapter.Adapters.ElectronicComponent
{
    public class Z2ElectronicComponentAdapter : IElectronicComponentAdapter<Z2ElectronicComponent>
    {

        public IElectronicComponent Adapt(Z2ElectronicComponent component)
        {
            var result = new Models.ElectronicComponent
            {
                Id = component.Id.ToString(),
                Warehouse = WarehouseType.Z2,
                PN = component.PN,
                MPN = component.MPN
            };
            if (component.Details != null)
            {
                result.DateCode = component.Details.DateCode;
                result.Lot = component.Details.Lot;
            }
            return result;
        }

    }
}
