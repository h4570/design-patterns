namespace decorator.ExchangeRates
{
    /// <summary>
    /// Component - interface shared by the real provider and all decorators.
    /// </summary>
    public interface IExchangeRateProvider
    {
        public Task<decimal> GetRateAsync(string from, string to, CancellationToken cancellationToken = default);
    }
}
