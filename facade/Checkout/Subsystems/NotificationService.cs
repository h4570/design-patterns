namespace facade.Checkout.Subsystems
{
    /// <summary>
    /// Subsystem - customer e-mails.
    /// </summary>
    public class NotificationService
    {

        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ILogger<NotificationService> logger)
        {
            _logger = logger;
        }

        public void SendOrderConfirmation(string email, string orderId, string trackingNumber, string invoiceNumber)
        {
            _logger.LogInformation("E-mail to {Email}: order {OrderId} confirmed, tracking {Tracking}, invoice {Invoice}",
                email, orderId, trackingNumber, invoiceNumber);
        }

        public void SendPaymentFailed(string email, string orderId, string reason)
        {
            _logger.LogInformation("E-mail to {Email}: payment for order {OrderId} failed - {Reason}", email, orderId, reason);
        }

    }
}
