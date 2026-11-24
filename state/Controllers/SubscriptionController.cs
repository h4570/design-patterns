using Microsoft.AspNetCore.Mvc;
using state.Subscriptions;

namespace state.Controllers
{
    public record Payment(decimal Amount);

    [ApiController]
    [Route("[controller]")]
    public class SubscriptionController : ControllerBase
    {

        private static readonly object Lock = new();
        private readonly Subscription _subscription;

        public SubscriptionController(Subscription subscription)
        {
            _subscription = subscription;
        }

        [HttpGet]
        public ActionResult Get()
        {
            lock (Lock) return Ok(Show(_subscription));
        }

        [HttpPost("pay")]
        public ActionResult Pay(Payment payment) => Run(c => c.Pay(payment.Amount));

        [HttpPost("payment-failed")]
        public ActionResult PaymentFailed() => Run(c => c.PaymentFailed());

        [HttpPost("cancel")]
        public ActionResult Cancel() => Run(c => c.Cancel());

        [HttpPost("reactivate")]
        public ActionResult Reactivate() => Run(c => c.Reactivate());

        [HttpPost("days/{days:int:range(1,365)}")]
        public ActionResult PassDays(int days) => Run(c => c.PassDays(days));

        /// <summary>
        /// Premium feature - whether it works depends only on the current state.
        /// </summary>
        [HttpGet("export-report")]
        public ActionResult ExportReport()
        {
            lock (Lock)
            {
                if (!_subscription.CanUsePremiumFeatures)
                    return StatusCode(StatusCodes.Status402PaymentRequired, $"Subscription is {_subscription.State.Name} - please pay to export reports");
                return Ok($"Sales report for {_subscription.Customer} generated");
            }
        }

        /// <summary>
        /// One year of a customer's life in a few steps, on a separate subscription.
        /// </summary>
        [HttpGet("demo")]
        public ActionResult Demo()
        {
            var subscription = new Subscription("Bakery Sp. z o.o.", new DateOnly(2026, 1, 1));
            var steps = new List<object>();

            void Step(string action, Action<Subscription> operation)
            {
                string result;
                try
                {
                    operation(subscription);
                    result = "OK";
                }
                catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
                {
                    result = ex.Message;
                }
                steps.Add(new
                {
                    Action = action,
                    Result = result,
                    subscription.Today,
                    State = subscription.State.Name,
                    subscription.PaidUntil,
                    PremiumFeatures = subscription.CanUsePremiumFeatures ? "available" : "blocked"
                });
            }

            Step("Sign up", _ => { });
            Step("14 days pass without payment", c => c.PassDays(14));
            Step("Pay 99 PLN", c => c.Pay(99));
            Step("A month passes", c => c.PassDays(31));
            Step("Card payment fails", c => c.PaymentFailed());
            Step("Pay 99 PLN with another card", c => c.Pay(99));
            Step("Cancel", c => c.Cancel());
            Step("Reactivate", c => c.Reactivate());
            Step("Cancel again", c => c.Cancel());
            Step("A month passes", c => c.PassDays(31));
            Step("Pay 99 PLN", c => c.Pay(99));

            return Ok(new { Steps = steps, subscription.History });
        }

        private ActionResult Run(Action<Subscription> operation)
        {
            lock (Lock)
            {
                try
                {
                    operation(_subscription);
                    return Ok(Show(_subscription));
                }
                catch (InvalidOperationException ex) { return Conflict(ex.Message); }
                catch (ArgumentException ex) { return BadRequest(ex.Message); }
            }
        }

        private static object Show(Subscription subscription) => new
        {
            subscription.Customer,
            State = subscription.State.Name,
            subscription.Today,
            subscription.DaysInState,
            subscription.PaidUntil,
            subscription.FailedPayments,
            subscription.CanUsePremiumFeatures,
            subscription.History
        };

    }
}
