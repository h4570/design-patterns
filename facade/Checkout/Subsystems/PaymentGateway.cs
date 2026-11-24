namespace facade.Checkout.Subsystems
{
    public class PaymentDeclinedException : Exception
    {
        public PaymentDeclinedException(string message) : base(message) { }
    }

    /// <summary>
    /// Subsystem - fake card payment provider.
    /// Card ending with "0002" is always declined (just like test cards in real payment gateways).
    /// </summary>
    public class PaymentGateway
    {

        /// <returns>Payment id</returns>
        public string Charge(string cardNumber, decimal amount, string description)
        {
            if (string.IsNullOrWhiteSpace(cardNumber) || cardNumber.Length < 12)
                throw new PaymentDeclinedException("Invalid card number");
            if (cardNumber.EndsWith("0002"))
                throw new PaymentDeclinedException("Card declined: insufficient funds");

            return $"PAY-{Guid.NewGuid().ToString()[..8].ToUpper()}";
        }

        public void Refund(string paymentId) { /* Call payment provider API */ }

    }
}
