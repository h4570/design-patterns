using facade.Checkout.Subsystems;

namespace facade.Checkout
{
    /// <summary>
    /// Facade - single, simple entry point to the whole checkout process.
    /// It knows in which order subsystems have to be called and how to roll back when something goes wrong.
    /// Clients (controllers, background jobs, admin panel...) just call PlaceOrder().
    /// </summary>
    public class CheckoutFacade
    {

        private readonly InventoryService _inventory;
        private readonly PaymentGateway _payments;
        private readonly ShippingService _shipping;
        private readonly InvoiceService _invoices;
        private readonly NotificationService _notifications;

        public CheckoutFacade(
            InventoryService inventory,
            PaymentGateway payments,
            ShippingService shipping,
            InvoiceService invoices,
            NotificationService notifications)
        {
            _inventory = inventory;
            _payments = payments;
            _shipping = shipping;
            _invoices = invoices;
            _notifications = notifications;
        }

        public CheckoutResult PlaceOrder(OrderRequest request)
        {
            var orderId = $"ORD-{DateTime.Now:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";

            // 1. Reserve products, so nobody else can buy them during payment
            string reservationId;
            try
            {
                reservationId = _inventory.Reserve(request.Lines);
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
            {
                return CheckoutResult.Failed(orderId, ex.Message);
            }

            // 2. Calculate totals
            var productsTotal = request.Lines.Sum(c => _inventory.GetPrice(c.Sku) * c.Quantity);
            var shippingCost = _shipping.Quote(request.ShippingAddress, productsTotal);
            var total = productsTotal + shippingCost;

            // 3. Charge customer - on failure give products back to the warehouse
            string paymentId;
            try
            {
                paymentId = _payments.Charge(request.CardNumber, total, $"Order {orderId}");
            }
            catch (PaymentDeclinedException ex)
            {
                _inventory.Release(reservationId);
                _notifications.SendPaymentFailed(request.CustomerEmail, orderId, ex.Message);
                return CheckoutResult.Failed(orderId, ex.Message);
            }

            // 4. Payment is done - finalize everything else
            _inventory.Commit(reservationId);
            var trackingNumber = _shipping.CreateShipment(orderId, request.ShippingAddress);
            var invoiceNumber = _invoices.Issue(orderId, request.ShippingAddress, total);
            _notifications.SendOrderConfirmation(request.CustomerEmail, orderId, trackingNumber, invoiceNumber);

            return new CheckoutResult(true, orderId, productsTotal, shippingCost, total,
                paymentId, trackingNumber, invoiceNumber, null);
        }

    }
}
