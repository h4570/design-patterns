using System.Text.Json.Serialization;
using chain_of_responsibility.Handlers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(c => c.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Order matters - cheap and automatic handlers first, humans last
builder.Services.AddSingleton<TicketHandler>(_ =>
{
    var chain = new SpamFilterHandler();
    chain.SetNext(new EnterpriseCustomerHandler())
         .SetNext(new FaqBotHandler())
         .SetNext(new AutoRefundHandler(limit: 200m))
         .SetNext(new HumanSupportHandler());
    return chain;
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
