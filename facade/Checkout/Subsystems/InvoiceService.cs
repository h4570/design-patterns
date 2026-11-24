namespace facade.Checkout.Subsystems
{
    /// <summary>
    /// Subsystem - accounting / invoicing.
    /// </summary>
    public class InvoiceService
    {

        private int _counter;

        /// <returns>Invoice number</returns>
        public string Issue(string orderId, Address billingAddress, decimal total)
        {
            var number = Interlocked.Increment(ref _counter);
            return $"FV/{number:D4}/{DateTime.Now:MM'/'yyyy}";
        }

    }
}
