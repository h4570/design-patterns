using strategy.Loans;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// New way of repayment = new class + one line here, nothing else changes
builder.Services.AddSingleton<IRepaymentStrategy, EqualInstallmentsStrategy>();
builder.Services.AddSingleton<IRepaymentStrategy, DecreasingInstallmentsStrategy>();
builder.Services.AddSingleton<IRepaymentStrategy>(new BalloonPaymentStrategy(balloonPercent: 30m));
builder.Services.AddSingleton<LoanCalculator>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
