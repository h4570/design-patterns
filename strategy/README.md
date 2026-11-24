# Strategy pattern

In this example strategy pattern was used in a **loan calculator** of a bank, which shows repayment schedules for different ways of paying back a loan:
- **equal installments** ("raty równe") - the same payment every month,
- **decreasing installments** ("raty malejące") - the same part of principal every month, payment goes down, cheaper in total,
- **balloon payment** - low installments and 30% of the loan paid in the last month, popular in car loans.

Every way is a separate class implementing `IRepaymentStrategy`. `LoanCalculator` gets all of them from DI and picks one by key - it has no `if`/`switch` on loan type.  
Adding a new strategy (e.g. "3 months without installments") is a new class and one line in Program.cs.  
To test this example, run "**/loan/demo**" endpoint (car loan: 60 000 PLN, 9.9%, 48 months), or:  
`GET /loan/strategies`, `GET /loan/schedule?strategy=decreasing&amount=60000&rate=9.9&months=48`, `GET /loan/compare?amount=500000&rate=7.2&months=360`.  
Please look on **RepaymentStrategies.cs** and **LoanCalculator.cs** to get better view of this implementation.

## Participants

- `IRepaymentStrategy` - **strategy**
- `EqualInstallmentsStrategy`, `DecreasingInstallmentsStrategy`, `BalloonPaymentStrategy` - **concrete strategies**
- `RepaymentStrategyBase` - shared math (interest, rounding, last installment), strategies only say how much principal is paid in a month
- `LoanCalculator` - **context**, chooses a strategy by key
- `LoanController` - **client**

### Strategies
```csharp
public class EqualInstallmentsStrategy : RepaymentStrategyBase
{
    public override string Key => "equal";

    protected override decimal PrincipalFor(LoanRequest loan, decimal monthlyRate, int month, decimal interest) =>
        AnnuityPayment(loan.Amount, monthlyRate, loan.Months) - interest;
}

public class DecreasingInstallmentsStrategy : RepaymentStrategyBase
{
    public override string Key => "decreasing";

    protected override decimal PrincipalFor(LoanRequest loan, decimal monthlyRate, int month, decimal interest) =>
        Math.Round(loan.Amount / loan.Months, 2);
}

public class BalloonPaymentStrategy : RepaymentStrategyBase
{
    public override string Key => "balloon";

    protected override decimal PrincipalFor(LoanRequest loan, decimal monthlyRate, int month, decimal interest) =>
        AnnuityPayment(loan.Amount, monthlyRate, loan.Months, loan.Amount * _balloonPercent / 100m) - interest;
}
```

### Context
```csharp
public RepaymentSchedule Calculate(LoanRequest loan, string strategyKey)
{
    if (!_strategies.TryGetValue(strategyKey ?? "", out var strategy))
        throw new ArgumentException($"Unknown strategy '{strategyKey}'. Available: {string.Join(", ", _strategies.Keys)}");
    return strategy.Calculate(loan);
}
```

### Registration
```csharp
// New way of repayment = new class + one line here, nothing else changes
builder.Services.AddSingleton<IRepaymentStrategy, EqualInstallmentsStrategy>();
builder.Services.AddSingleton<IRepaymentStrategy, DecreasingInstallmentsStrategy>();
builder.Services.AddSingleton<IRepaymentStrategy>(new BalloonPaymentStrategy(balloonPercent: 30m));
builder.Services.AddSingleton<LoanCalculator>();
```

### Result
`/loan/demo` - car loan, 60 000 PLN, 9.9%, 48 months:

| Strategy | First payment | Last payment | Total interest | Total paid |
|---|---|---|---|---|
| equal | 1518.88 | 1518.60 | 12905.96 | 72905.96 |
| decreasing | 1745.00 | 1260.31 | **12127.50** | 72127.50 |
| balloon | **1211.71** | 19211.87 | 16162.24 | 76162.24 |

`/loan/compare?amount=500000&rate=7.2&months=360` - mortgage, 30 years:

| Strategy | First payment | Last payment | Total interest | Total paid |
|---|---|---|---|---|
| equal | 3393.94 | 3395.22 | 721819.68 | 1221819.68 |
| decreasing | 4388.89 | 1396.82 | **541499.63** | 1041499.63 |
| balloon | **3275.76** | 153274.13 | 829271.97 | 1329271.97 |

Same input, three different algorithms - the client only changes the `strategy` parameter.

## Where else you can find it

`IComparer<T>` passed to `List.Sort`, `IPasswordHasher<T>` in ASP.NET Core Identity, and authentication schemes (cookie, JWT) are all strategies.  
Very often a strategy is just a delegate - `Func<LoanRequest, RepaymentSchedule>` would work too, a class is better when it has a name, a description and dependencies.
