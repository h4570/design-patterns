namespace decorator.ExchangeRates.Decorators
{
    /// <summary>
    /// Retries transient HTTP failures with exponential backoff.
    /// </summary>
    public class RetryExchangeRateDecorator : ExchangeRateProviderDecorator
    {

        private readonly int _maxAttempts;
        private readonly ILogger _logger;

        public RetryExchangeRateDecorator(IExchangeRateProvider inner, ILogger logger, int maxAttempts = 5) : base(inner)
        {
            _logger = logger;
            _maxAttempts = maxAttempts;
        }

        public override async Task<decimal> GetRateAsync(string from, string to, CancellationToken cancellationToken = default)
        {
            for (var attempt = 1; ; attempt++)
            {
                try
                {
                    return await base.GetRateAsync(from, to, cancellationToken);
                }
                catch (HttpRequestException ex) when (attempt < _maxAttempts)
                {
                    var delay = TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt - 1));
                    _logger.LogWarning("Attempt {Attempt}/{Max} failed ({Message}), retrying in {Delay} ms",
                        attempt, _maxAttempts, ex.Message, delay.TotalMilliseconds);
                    await Task.Delay(delay, cancellationToken);
                }
            }
        }

    }
}
