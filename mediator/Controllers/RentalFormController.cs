using mediator.RentalForm;
using Microsoft.AspNetCore.Mvc;

namespace mediator.Controllers
{
    [ApiController]
    [Route("rental-form")]
    public class RentalFormController : ControllerBase
    {

        /// <summary>
        /// Applies changes one by one to a new form and returns the form after every change - like a user clicking through it.
        /// </summary>
        [HttpPost]
        public ActionResult<IReadOnlyList<FormState>> Fill(IReadOnlyList<FieldChange> changes)
        {
            var form = new CarRentalForm();
            var states = new List<FormState>();
            try
            {
                foreach (var change in changes)
                    states.Add(form.Apply(change));
                return Ok(states);
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
            {
                return BadRequest(new { Error = ex.Message, StatesBeforeError = states });
            }
        }

        [HttpGet("demo")]
        public ActionResult<IReadOnlyList<FormState>> Demo() => Fill(new FieldChange[]
        {
            new("pickupStation", "Warsaw Chopin Airport"),
            new("dates", "2026-12-20/2026-12-27"),
            new("carClass", "Premium"),
            new("driverAge", "23"),
            new("fullInsurance", "false"),
            new("returnStation", "Gdansk Main Station")
        });

    }
}
