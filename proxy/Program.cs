using proxy.Documents;
using proxy.Users;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CurrentUser>();

// Real subject - one shared instance, created on first use (thread safe thanks to Lazy<T>)
builder.Services.AddSingleton<CloudDocumentStorage>();
builder.Services.AddSingleton(sp => new Lazy<CloudDocumentStorage>(sp.GetRequiredService<CloudDocumentStorage>));

// Clients asking for IDocumentStorage get the proxy, never the real storage
builder.Services.AddScoped<IDocumentStorage, DocumentStorageProxy>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
