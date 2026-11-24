namespace facade.Checkout
{
    public record OrderLine(string Sku, int Quantity);

    public record Address(string FullName, string Street, string City, string PostalCode, string Country);

    public record OrderRequest(
        string CustomerEmail,
        IReadOnlyList<OrderLine> Lines,
        Address ShippingAddress,
        string CardNumber);

    public record CheckoutResult(
        bool Success,
        string OrderId,
        decimal ProductsTotal,
        decimal ShippingCost,
        decimal Total,
        string PaymentId,
        string TrackingNumber,
        string InvoiceNumber,
        string Error)
    {
        public static CheckoutResult Failed(string orderId, string error) =>
            new(false, orderId, 0, 0, 0, null, null, null, error);
    }
}
