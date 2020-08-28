using adapter.Models;
using adapter.Models.WarehouseZ1;

namespace adapter.Z_Bonus2
{

    /// <summary>
    /// Z1 Warehouse adapter
    /// </summary>
    public partial class ElectronicComponentAdapter : IElectronicComponent
    {

        private void AdaptZ1Component(Z1ElectronicComponent component)
        {
            if (component.Id != null)
                Id = component.Id.ToString();
            Warehouse = WarehouseType.Z1;
            MPN = component.MPN;
            ParseZ1Barcode(component);
        }

        private void ParseZ1Barcode(Z1ElectronicComponent component)
        {
            if (component.Barcode != null && component.Barcode.Contains(';'))
            {
                var splittedBarcode = component.Barcode.Split(';');
                PN = GetSplittedZ1BarcodePart(splittedBarcode, 0);
                Lot = GetSplittedZ1BarcodePart(splittedBarcode, 1);
                DateCode = GetSplittedZ1BarcodePart(splittedBarcode, 2);
            }
        }

        private string GetSplittedZ1BarcodePart(string[] splittedBarcode, int index)
        {
            if (splittedBarcode.Length >= index)
                if (splittedBarcode[index].Trim() != string.Empty)
                    return splittedBarcode[index].Trim();
            return null;
        }

    }

}
