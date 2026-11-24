using template_method.Imports;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<ProductRepository>();
builder.Services.AddSingleton<SupplierImport, KowalskiCsvImport>();
builder.Services.AddSingleton<SupplierImport, TechDistriJsonImport>();
builder.Services.AddSingleton<SupplierImport, ElektroXmlImport>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
