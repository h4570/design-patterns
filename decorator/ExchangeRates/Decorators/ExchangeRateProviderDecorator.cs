namespace decorator.ExchangeRates.Decorators
{
    /// <summary>
    /// Base decorator - keeps reference to wrapped provider and delegates to it by default.
    /// </summary>
    public abstract class ExchangeRateProviderDecorator : IExchangeRateProvider
    {

        protected readonly IExchangeRateProvider Inner;

        protected ExchangeRateProviderDecorator(IExchangeRateProvider inner)
        {
            Inner = inner;
        }

        public virtual Task<decimal> GetRateAsync(string from, string to, CancellationToken cancellationToken = default)
        {
            return Inner.GetRateAsync(from, to, cancellationToken);
        }

    }
}
