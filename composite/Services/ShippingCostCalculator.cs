using composite.Packaging;

namespace composite.Services
{
    /// <summary>
    /// Client of the composite. It works only with IPackageItem,
    /// so it can price a single product as well as a pallet full of nested boxes.
    /// </summary>
    public class ShippingCostCalculator
    {

        private const decimal BASE_PRICE = 12.99m;
        private const decimal PRICE_PER_KG = 1.50m;
        private const decimal INSURANCE_PERCENTAGE = 1;

        public decimal Calculate(IPackageItem package)
        {
            var weightCost = Math.Ceiling(package.GetWeightKg()) * PRICE_PER_KG;
            var insurance = package.GetPrice() * (INSURANCE_PERCENTAGE / 100);
            return Math.Round(BASE_PRICE + weightCost + insurance, 2);
        }

    }
}
