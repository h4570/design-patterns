using Microsoft.AspNetCore.Mvc;
using strategy.Loans;

namespace strategy.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoanController : ControllerBase
    {

        private readonly LoanCalculator _calculator;

        public LoanController(LoanCalculator calculator)
        {
            _calculator = calculator;
        }

        [HttpGet("strategies")]
        public ActionResult GetStrategies() => Ok(_calculator.Strategies.Select(c => new { c.Key, c.Description }));

        /// <summary>
        /// Full schedule, e.g. /loan/schedule?strategy=decreasing&amount=60000&rate=9.9&months=48
        /// </summary>
        [HttpGet("schedule")]
        public ActionResult<RepaymentSchedule> GetSchedule(string strategy, decimal amount, decimal rate, int months)
        {
            try
            {
                return Ok(_calculator.Calculate(new LoanRequest(amount, rate, months), strategy));
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
        }

        /// <summary>
        /// All strategies side by side, e.g. /loan/compare?amount=500000&rate=7.2&months=360
        /// </summary>
        [HttpGet("compare")]
        public ActionResult Compare(decimal amount, decimal rate, int months)
        {
            try
            {
                return Ok(Summary(_calculator.Compare(new LoanRequest(amount, rate, months))));
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
        }

        /// <summary>
        /// Car loan: 60 000 PLN, 9.9%, 4 years.
        /// </summary>
        [HttpGet("demo")]
        public ActionResult Demo() => Compare(60000m, 9.9m, 48);

        private static IEnumerable<object> Summary(IEnumerable<RepaymentSchedule> schedules) => schedules.Select(c => new
        {
            c.Strategy,
            c.FirstPayment,
            c.LastPayment,
            c.TotalInterest,
            c.TotalPaid,
            FirstMonths = c.Installments.Take(3)
        });

    }
}
