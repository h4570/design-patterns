using state.Subscriptions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton(_ => new Subscription("Bakery Sp. z o.o.", DateOnly.FromDateTime(DateTime.Today)));

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
