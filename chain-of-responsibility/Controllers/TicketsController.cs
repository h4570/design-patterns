using chain_of_responsibility.Handlers;
using chain_of_responsibility.Tickets;
using Microsoft.AspNetCore.Mvc;

namespace chain_of_responsibility.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TicketsController : ControllerBase
    {

        // Controller knows only the first link of the chain
        private readonly TicketHandler _triage;

        public TicketsController(TicketHandler triage)
        {
            _triage = triage;
        }

        [HttpPost]
        public ActionResult<TicketResolution> Submit(Ticket ticket) => Ok(_triage.Handle(ticket));

        /// <summary>
        /// Browser friendly example - sends a few typical tickets through the chain.
        /// </summary>
        [HttpGet("demo")]
        public ActionResult<object> Demo()
        {
            var tickets = new[]
            {
                new Ticket("winner@spam.io", CustomerPlan.Free, "Crypto giveaway!!!", "Click here to double your BTC"),
                new Ticket("ceo@bigcorp.com", CustomerPlan.Enterprise, "Reset password", "I forgot password to the admin panel"),
                new Ticket("john@example.com", CustomerPlan.Free, "Login problem", "I forgot password, please help"),
                new Ticket("anna@example.com", CustomerPlan.Pro, "Refund", "I was charged twice, please refund 49.99 PLN"),
                new Ticket("mark@example.com", CustomerPlan.Pro, "Refund for annual plan", "Please refund 1200 PLN, I bought the wrong plan"),
                new Ticket("eve@example.com", CustomerPlan.Free, "Export to Excel", "Exported report has empty columns"),
            };

            return Ok(tickets.Select(c => new { c.Subject, c.Plan, Resolution = _triage.Handle(c) }));
        }

    }
}
