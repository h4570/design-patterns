namespace decorator.ExchangeRates
{
    /// <summary>
    /// Concrete component - simulates slow and unreliable external bank API.
    /// It knows nothing about caching, retrying or logging - and it should stay that way.
    /// </summary>
    public class ExternalBankExchangeRateProvider : IExchangeRateProvider
    {

        private const double FAILURE_CHANCE = 0.4;
        private static readonly TimeSpan _latency = TimeSpan.FromMilliseconds(400);

        private static readonly Dictionary<string, decimal> _ratesToPln = new()
        {
            ["PLN"] = 1m,
            ["EUR"] = 4.2650m,
            ["USD"] = 3.9120m,
            ["GBP"] = 5.0410m,
            ["CHF"] = 4.5380m
        };

        public async Task<decimal> GetRateAsync(string from, string to, CancellationToken cancellationToken = default)
        {
            await Task.Delay(_latency, cancellationToken);

            if (Random.Shared.NextDouble() < FAILURE_CHANCE)
                throw new HttpRequestException("External bank API returned 503 Service Unavailable");

            if (!_ratesToPln.TryGetValue(from, out var fromRate) || !_ratesToPln.TryGetValue(to, out var toRate))
                throw new ArgumentException($"Unsupported currency pair {from}/{to}");

            return Math.Round(fromRate / toRate, 4);
        }

    }
}
