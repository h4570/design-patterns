namespace facade.Checkout.Subsystems
{
    /// <summary>
    /// Subsystem - courier integration.
    /// </summary>
    public class ShippingService
    {

        private const decimal FREE_SHIPPING_THRESHOLD = 500m;

        public decimal Quote(Address address, decimal productsTotal)
        {
            if (productsTotal >= FREE_SHIPPING_THRESHOLD) return 0m;
            return address.Country == "PL" ? 14.99m : 49.99m;
        }

        /// <returns>Tracking number</returns>
        public string CreateShipment(string orderId, Address address)
        {
            return $"TRK{Random.Shared.Next(100_000_000, 999_999_999)}{address.Country}";
        }

    }
}
