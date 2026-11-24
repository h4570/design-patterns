# Chain of responsibility pattern

In this example chain of responsibility pattern was used for **triage of customer support tickets** in a SaaS application.  
Every ticket goes through a chain of handlers: *spam filter* → *enterprise customer* → *FAQ bot* → *automatic refund* → *human support*.  
Each handler decides on its own: it either resolves the ticket, or passes it to the next one - without knowing who is next.  
Cheap and automatic handlers are first, people are last, so support team gets only tickets which really need a human.  
To test this example, run "**/tickets/demo**" endpoint or send your own ticket with `POST /tickets`.  
Please look on **TicketHandler.cs** and **Program.cs** to get better view of this implementation.

## Participants

- `TicketHandler` - **handler**, abstract base which keeps the link to the next handler and passes the ticket further
- `SpamFilterHandler`, `EnterpriseCustomerHandler`, `FaqBotHandler`, `AutoRefundHandler` - **concrete handlers**, each handles only what it understands
- `HumanSupportHandler` - catch-all at the end of the chain, so no ticket is lost
- `TicketsController` - **client**, knows only the first link

### Handler implementation
```csharp
private TicketResolution Handle(Ticket ticket, List<string> trail)
{
    trail.Add(Name);

    var result = TryHandle(ticket);
    if (result != null)
        return new TicketResolution(Name, result.Value.Status, result.Value.Reply, trail);

    if (_next == null)
        throw new InvalidOperationException("Nobody handled the ticket - chain should end with a catch-all handler");
    return _next.Handle(ticket, trail);
}

/// <returns>Status and reply when the ticket was handled, null to pass it to the next handler</returns>
protected abstract (string Status, string Reply)? TryHandle(Ticket ticket);
```

### Example handler
```csharp
public class AutoRefundHandler : TicketHandler
{
    protected override (string Status, string Reply)? TryHandle(Ticket ticket)
    {
        if (!ticket.Text.Contains("refund")) return null;

        var amount = Regex.Match(ticket.Text, @"(\d+(?:[.,]\d{1,2})?)\s*(pln|zł|zl)");
        if (!amount.Success) return null;

        var value = decimal.Parse(amount.Groups[1].Value.Replace(',', '.'), CultureInfo.InvariantCulture);
        if (value > _limit) return null;   // too much money for a robot - next handler decides

        return ("RefundApproved", $"Refund of {value:0.00} PLN was approved...");
    }
}
```

### Building the chain
```csharp
// Order matters - cheap and automatic handlers first, humans last
var chain = new SpamFilterHandler();
chain.SetNext(new EnterpriseCustomerHandler())
     .SetNext(new FaqBotHandler())
     .SetNext(new AutoRefundHandler(limit: 200m))
     .SetNext(new HumanSupportHandler());
```

### Client code
```csharp
[HttpPost]
public ActionResult<TicketResolution> Submit(Ticket ticket) => Ok(_triage.Handle(ticket));
```

### Result (`/tickets/demo`, shortened)
| Ticket | Plan | Handled by | Status | Trail |
|---|---|---|---|---|
| Crypto giveaway!!! | Free | SpamFilterHandler | Rejected | Spam |
| Reset password | Enterprise | EnterpriseCustomerHandler | AssignedToAccountManager | Spam → Enterprise |
| I forgot password | Free | FaqBotHandler | AutoAnswered | Spam → Enterprise → Faq |
| Refund 49.99 PLN | Pro | AutoRefundHandler | RefundApproved | Spam → Enterprise → Faq → Refund |
| Refund 1200 PLN | Pro | HumanSupportHandler | QueuedForSupport | Spam → Enterprise → Faq → Refund → Human |
| Export to Excel | Free | HumanSupportHandler | QueuedForSupport | Spam → Enterprise → Faq → Refund → Human |

Notice that the enterprise customer asking to reset password never reaches the FAQ bot - order of the chain is a business decision.

## Where else you can find it

ASP.NET Core middleware pipeline is a chain of responsibility - every middleware can return the response or call `next()`.  
The same goes for `DelegatingHandler` in `HttpClient`.
