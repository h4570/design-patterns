using adapter.Models;
using adapter.Models.WarehouseZ1;

namespace adapter.Z_Bonus1
{

    public class Z1ElectronicComponentAdapter : IElectronicComponent
    {

        public string Id {
            get {
                if (_originalComponent.Id != null)
                    return _originalComponent.Id.ToString();
                else return null;
            }
        }
        public WarehouseType Warehouse => WarehouseType.Z1;
        public string PN { get; private set; }
        public string MPN => _originalComponent.MPN;
        public string DateCode { get; private set; }
        public string Lot { get; private set; }

        private readonly Z1ElectronicComponent _originalComponent;

        public Z1ElectronicComponentAdapter(Z1ElectronicComponent originalComponent)
        {
            _originalComponent = originalComponent;
            ParseBarcode();
        }

        private void ParseBarcode()
        {
            if (_originalComponent.Barcode != null && _originalComponent.Barcode.Contains(';'))
            {
                var splittedBarcode = _originalComponent.Barcode.Split(';');
                PN = GetSplittedBarcodePart(splittedBarcode, 0);
                Lot = GetSplittedBarcodePart(splittedBarcode, 1);
                DateCode = GetSplittedBarcodePart(splittedBarcode, 2);
            }
        }

        private string GetSplittedBarcodePart(string[] splittedBarcode, int index)
        {
            if (splittedBarcode.Length >= index)
                if (splittedBarcode[index].Trim() != string.Empty)
                    return splittedBarcode[index].Trim();
            return null;
        }

    }

}
