# State pattern

In this example state pattern was used for **subscription lifecycle in a SaaS application** (99 PLN / month):

| State | Premium features | Pay | Card failed | Cancel | Reactivate | Time passes |
|---|---|---|---|---|---|---|
| **Trial** | yes | → Active | - | → Cancelled | - | 14 days → Suspended |
| **Active** | yes | - | → PastDue | → PendingCancellation | - | end of period → PastDue |
| **PastDue** | yes (grace) | → Active | 3rd failure → Suspended | → Cancelled | - | 7 days → Suspended |
| **Suspended** | no | → Active | - | → Cancelled | - | 30 days → Cancelled |
| **PendingCancellation** | yes | - | - | - | → Active | end of period → Cancelled |
| **Cancelled** | no | - | - | - | - | - |

`-` means "not possible" - the operation returns `409 Conflict`.

The same action means something different in every state: *pay* during trial starts the first period, in *PastDue* continues the old one (paying late gives no extra days), in *Cancelled* is not allowed at all.  
*Cancel* in *Active* keeps access until the end of the paid month, in *Trial* ends immediately. Premium features work in some states and not in others.  
Instead of a `switch (status)` in every method, each state is a class, and `Subscription` delegates every call to its current state. States decide themselves which state comes next.  
To test this example, run "**/subscription/demo**" endpoint (one year of a customer on a separate subscription), or play with the shared one:  
`POST /subscription/days/14`, `POST /subscription/pay` with `{"amount":99}`, `POST /subscription/payment-failed`, `POST /subscription/cancel`, `POST /subscription/reactivate`, `GET /subscription/export-report`.  
Please look on **SubscriptionStates.cs** to get better view of this implementation.

## Participants

- `Subscription` - **context**, keeps the current state and delegates to it
- `SubscriptionState` - **state**, by default every operation throws "not possible"
- `TrialState`, `ActiveState`, `PastDueState`, `SuspendedState`, `PendingCancellationState`, `CancelledState` - **concrete states**, override only what makes sense for them
- `SubscriptionController` - **client**

### Context
```csharp
public void Pay(decimal amount)
{
    if (amount != MonthlyPrice)
        throw new ArgumentException($"Monthly price is {MonthlyPrice} PLN");
    State.Pay(this);
}

public void Cancel() => State.Cancel(this);
public bool CanUsePremiumFeatures => State.CanUsePremiumFeatures;

internal void TransitionTo(SubscriptionState next, string reason)
{
    _history.Add(new StateChange(Today, State.Name, next.Name, reason));
    State = next;
    DaysInState = 0;
}
```

### Concrete state
```csharp
public class PastDueState : SubscriptionState
{
    public override string Name => "PastDue";
    public override bool CanUsePremiumFeatures => true;   // grace period

    public override void Pay(Subscription subscription)
    {
        // New period continues from the old one - paying late does not give extra days
        StartNewPeriod(subscription, subscription.PaidUntil ?? subscription.Today);
        subscription.TransitionTo(new ActiveState(), "Overdue payment received");
    }

    public override void PaymentFailed(Subscription subscription)
    {
        subscription.FailedPayments++;
        if (subscription.FailedPayments >= MaxFailedPayments)
            subscription.TransitionTo(new SuspendedState(), $"{MaxFailedPayments} failed payments");
    }

    public override void DayPassed(Subscription subscription)
    {
        if (subscription.DaysInState >= GraceDays)
            subscription.TransitionTo(new SuspendedState(), $"Not paid within {GraceDays} days");
    }
}
```
`Reactivate` is not overridden - base class throws `Reactivation is not possible when subscription is PastDue`.

### Client code
```csharp
[HttpGet("export-report")]
public ActionResult ExportReport()
{
    if (!_subscription.CanUsePremiumFeatures)
        return StatusCode(StatusCodes.Status402PaymentRequired, $"Subscription is {_subscription.State.Name} - please pay to export reports");
    return Ok($"Sales report for {_subscription.Customer} generated");
}
```

### Result (`/subscription/demo`)
| Action | Result | Date | State | Paid until | Premium features |
|---|---|---|---|---|---|
| Sign up | OK | 2026-01-01 | Trial | - | available |
| 14 days pass without payment | OK | 2026-01-15 | Suspended | - | **blocked** |
| Pay 99 PLN | OK | 2026-01-15 | Active | 2026-02-15 | available |
| A month passes | OK | 2026-02-15 | PastDue | 2026-02-15 | available |
| Card payment fails | OK | 2026-02-15 | PastDue | 2026-02-15 | available |
| Pay 99 PLN with another card | OK | 2026-02-15 | Active | 2026-03-15 | available |
| Cancel | OK | 2026-02-15 | PendingCancellation | 2026-03-15 | available |
| Reactivate | OK | 2026-02-15 | Active | 2026-03-15 | available |
| Cancel again | OK | 2026-02-15 | PendingCancellation | 2026-03-15 | available |
| A month passes | OK | 2026-03-18 | Cancelled | 2026-03-15 | **blocked** |
| Pay 99 PLN | Payment is not possible when subscription is Cancelled | 2026-03-18 | Cancelled | 2026-03-15 | **blocked** |

## State vs Strategy

Both replace `if`/`switch` with objects. Strategy is chosen **from outside** and usually does not change by itself (see strategy example).  
States **replace each other** - the current state decides what comes next. Adding a new state (e.g. *Paused*) means a new class and a few transitions, without touching the others.
