using System.Diagnostics;

namespace decorator.ExchangeRates.Decorators
{
    /// <summary>
    /// Measures and logs every call (including time spent in all inner decorators).
    /// </summary>
    public class LoggingExchangeRateDecorator : ExchangeRateProviderDecorator
    {

        private readonly ILogger _logger;

        public LoggingExchangeRateDecorator(IExchangeRateProvider inner, ILogger logger) : base(inner)
        {
            _logger = logger;
        }

        public override async Task<decimal> GetRateAsync(string from, string to, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var rate = await base.GetRateAsync(from, to, cancellationToken);
                _logger.LogInformation("{From}/{To} = {Rate} ({Elapsed} ms)", from, to, rate, stopwatch.ElapsedMilliseconds);
                return rate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{From}/{To} failed after {Elapsed} ms", from, to, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

    }
}
