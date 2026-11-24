using System.Globalization;

namespace visitor.Cart
{
    public record VatLine(string Item, string Rate, decimal Gross, decimal Net, decimal Vat);

    /// <summary>
    /// Polish VAT: 23% electronics and services, 5% books and ebooks.
    /// Gift card is not a sale yet - VAT is paid when it is used, so it is "NP" (not subject to VAT).
    /// </summary>
    public class VatCalculator : ICartVisitor<VatLine>
    {

        public VatLine Visit(PhysicalProduct item) => FromGross(item.Name, item.GrossPrice * item.Quantity, item.VatPercent);
        public VatLine Visit(Ebook item) => FromGross(item.Name, item.GrossPrice, 5);
        public VatLine Visit(GiftCard item) => new(item.Name, "NP", item.Value, item.Value, 0m);
        public VatLine Visit(InstallationService item) => FromGross(item.Name, item.GrossPrice, 23);

        private static VatLine FromGross(string name, decimal gross, int percent)
        {
            var net = Math.Round(gross * 100m / (100m + percent), 2);
            return new VatLine(name, $"{percent}%", gross, net, gross - net);
        }

    }

    public record Delivery(string Item, string Method, decimal WeightKg);

    /// <summary>
    /// How every item gets to the customer - only physical products go into the parcel.
    /// </summary>
    public class DeliveryPlanner : ICartVisitor<Delivery>
    {

        public Delivery Visit(PhysicalProduct item) => new(item.Name, "Parcel", item.WeightKg * item.Quantity);
        public Delivery Visit(Ebook item) => new(item.Name, $"Download link ({item.Format}) by e-mail", 0m);
        public Delivery Visit(GiftCard item) => new(item.Name, $"Code sent to {item.RecipientEmail}", 0m);
        public Delivery Visit(InstallationService item) => new(item.Name, $"Technician visit in {item.City}", 0m);

    }

    /// <summary>
    /// Line for the accounting system. Gift card is a liability (we owe the goods), not revenue.
    /// </summary>
    public class AccountingExport : ICartVisitor<string>
    {

        private readonly VatCalculator _vat = new();

        public string Visit(PhysicalProduct item) => Line("700 Revenue - goods", item.Accept(_vat));
        public string Visit(Ebook item) => Line("701 Revenue - digital goods", item.Accept(_vat));
        public string Visit(GiftCard item) => Line("845 Liability - gift cards", item.Accept(_vat));
        public string Visit(InstallationService item) => Line("702 Revenue - services", item.Accept(_vat));

        private static string Line(string account, VatLine vat) =>
            string.Create(CultureInfo.InvariantCulture, $"{account}; {vat.Item}; net {vat.Net:0.00}; VAT {vat.Rate} {vat.Vat:0.00}");

    }
}
