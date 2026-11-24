# Decorator pattern

In this example decorator pattern was used to add **retrying, caching and logging** to a slow and unreliable external exchange rate API.  
`ExternalBankExchangeRateProvider` simulates a bank API which needs ~400 ms per call and fails with *503* in 40% of cases.  
Instead of putting all of that logic into one class, every concern lives in its own decorator, and all of them implement the same `IExchangeRateProvider` interface.  
Decorators are composed in **Program.cs**, so `ExchangeRateController` has no idea how many layers are wrapped around the real provider.  
To test this example, run "**/exchange-rate/EUR/PLN**" endpoint few times and watch the console logs.  
First call takes ~400 ms (sometimes more, when retry kicks in), next calls within 30 seconds take 0 ms - they are served from cache.

## Participants

- `IExchangeRateProvider` - **component**
- `ExternalBankExchangeRateProvider` - **concrete component**, fake external API
- `ExchangeRateProviderDecorator` - **base decorator**, delegates to wrapped provider
- `RetryExchangeRateDecorator`, `CachingExchangeRateDecorator`, `LoggingExchangeRateDecorator` - **concrete decorators**

## Decorator vs Inheritance

With inheritance you would need a class for every combination: `CachedProvider`, `CachedRetryProvider`, `LoggedCachedRetryProvider`...  
With decorators you have one class per feature and you can combine them freely at runtime, also with different order.  
Order matters - try to swap Caching and Retry in Program.cs and think what will change.

### Composition (Program.cs)
```csharp
IExchangeRateProvider provider = new ExternalBankExchangeRateProvider();
provider = new RetryExchangeRateDecorator(provider, retryLogger);
provider = new CachingExchangeRateDecorator(provider, cache, TimeSpan.FromSeconds(30));
provider = new LoggingExchangeRateDecorator(provider, logger);
return provider;
```

### Example decorator
```csharp
public override async Task<decimal> GetRateAsync(string from, string to, CancellationToken cancellationToken = default)
{
    var key = $"exchange-rate:{from}:{to}";

    if (_cache.TryGetValue(key, out decimal cachedRate))
        return cachedRate;

    var rate = await base.GetRateAsync(from, to, cancellationToken);
    _cache.Set(key, rate, _ttl);
    return rate;
}
```

### Console output
```
warn: RetryExchangeRateDecorator
      Attempt 1/5 failed (External bank API returned 503 Service Unavailable), retrying in 100 ms
warn: RetryExchangeRateDecorator
      Attempt 2/5 failed (External bank API returned 503 Service Unavailable), retrying in 200 ms
info: LoggingExchangeRateDecorator
      EUR/PLN = 4.2650 (1530 ms)
info: LoggingExchangeRateDecorator
      EUR/PLN = 4.2650 (0 ms)
```
