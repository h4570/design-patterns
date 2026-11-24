using System.Text.Json.Serialization;
using observer.Shipments;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(c => c.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddSingleton<NotificationLog>();
builder.Services.AddSingleton<ShipmentTracker>();
builder.Services.AddSingleton<DeliveryTimeAnalytics>();
builder.Services.AddSingleton(c => new ShopWebhookNotifier("https://fashion-shop.pl/webhooks/shipments", c.GetRequiredService<NotificationLog>()));

var app = builder.Build();

// Long living observers subscribe once at startup, customers' SMS notifiers come and go at runtime
var tracker = app.Services.GetRequiredService<ShipmentTracker>();
tracker.Subscribe(app.Services.GetRequiredService<DeliveryTimeAnalytics>());
tracker.Subscribe(app.Services.GetRequiredService<ShopWebhookNotifier>());

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
