using adapter.Models;
using adapter.Models.WarehouseZ1;
using System;
using System.Collections.Generic;

namespace adapter.Adapters.ElectronicComponent
{
    public class Z1ElectronicComponentAdapter : IElectronicComponentAdapter<Z1ElectronicComponent>
    {

        public IElectronicComponent Adapt(Z1ElectronicComponent component)
        {
            var result = new Models.ElectronicComponent();
            if (component.Id != null)
                result.Id = component.Id.ToString();
            result.Warehouse = WarehouseType.Z1;
            result.MPN = component.MPN;
            var (pn, lot, dateCode) = GetBarcodeVariables(component);
            result.PN = pn;
            result.Lot = lot;
            result.DateCode = dateCode;
            return result;
        }

        private (string pn, string lot, string dateCode) GetBarcodeVariables(Z1ElectronicComponent component)
        {
            if (component.Barcode != null && component.Barcode.Contains(';'))
            {
                var splittedBarcode = component.Barcode.Split(';');
                var pn = RemovePrefixIfExists(GetSplittedBarcodePart(splittedBarcode, 0));
                var lot = RemovePrefixIfExists(GetSplittedBarcodePart(splittedBarcode, 1));
                var dateCode = RemovePrefixIfExists(GetSplittedBarcodePart(splittedBarcode, 2));
                return (pn, lot, dateCode);
            }
            return (null, null, null);
        }

        private string RemovePrefixIfExists(string barcodeVar)
        {
            var prefixes = new List<string>() { "1P", "1T", "1D" };
            if (barcodeVar != null &&
                    barcodeVar.Length > 2 &&
                    prefixes.Contains(barcodeVar.Substring(0, 2).ToUpper()))
                return barcodeVar.Substring(2);
            return barcodeVar;
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
