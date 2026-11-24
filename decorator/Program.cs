using decorator.ExchangeRates;
using decorator.ExchangeRates.Decorators;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddMemoryCache();

// Decorators are composed here, in one place. Order matters:
// Logging -> Caching -> Retry -> External API
// - logging sees every request (also cache hits),
// - cache hit skips retrying and calling the API at all,
// - retry wraps only the real HTTP call.
builder.Services.AddSingleton<IExchangeRateProvider>(sp =>
{
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var cache = sp.GetRequiredService<IMemoryCache>();

    IExchangeRateProvider provider = new ExternalBankExchangeRateProvider();
    provider = new RetryExchangeRateDecorator(provider, loggerFactory.CreateLogger<RetryExchangeRateDecorator>());
    provider = new CachingExchangeRateDecorator(provider, cache, TimeSpan.FromSeconds(30));
    provider = new LoggingExchangeRateDecorator(provider, loggerFactory.CreateLogger<LoggingExchangeRateDecorator>());
    return provider;
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
