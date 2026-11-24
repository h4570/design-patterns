using Microsoft.Extensions.Caching.Memory;

namespace decorator.ExchangeRates.Decorators
{
    /// <summary>
    /// Caches rates for a short period, so the external API is called only when needed.
    /// </summary>
    public class CachingExchangeRateDecorator : ExchangeRateProviderDecorator
    {

        private readonly IMemoryCache _cache;
        private readonly TimeSpan _ttl;

        public CachingExchangeRateDecorator(IExchangeRateProvider inner, IMemoryCache cache, TimeSpan ttl) : base(inner)
        {
            _cache = cache;
            _ttl = ttl;
        }

        public override async Task<decimal> GetRateAsync(string from, string to, CancellationToken cancellationToken = default)
        {
            var key = $"exchange-rate:{from}:{to}";

            if (_cache.TryGetValue(key, out decimal cachedRate))
                return cachedRate;

            var rate = await base.GetRateAsync(from, to, cancellationToken);
            _cache.Set(key, rate, _ttl);
            return rate;
        }

    }
}
