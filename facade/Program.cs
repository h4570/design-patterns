using facade.Checkout;
using facade.Checkout.Subsystems;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Subsystems
builder.Services.AddSingleton<InventoryService>();
builder.Services.AddSingleton<PaymentGateway>();
builder.Services.AddSingleton<ShippingService>();
builder.Services.AddSingleton<InvoiceService>();
builder.Services.AddSingleton<NotificationService>();

// Facade
builder.Services.AddSingleton<CheckoutFacade>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
