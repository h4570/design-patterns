using System.Diagnostics;
using decorator.ExchangeRates;
using Microsoft.AspNetCore.Mvc;

namespace decorator.Controllers
{
    [ApiController]
    [Route("exchange-rate")]
    public class ExchangeRateController : ControllerBase
    {

        private readonly IExchangeRateProvider _exchangeRateProvider;

        // Controller does not know how many decorators are wrapped around the provider.
        public ExchangeRateController(IExchangeRateProvider exchangeRateProvider)
        {
            _exchangeRateProvider = exchangeRateProvider;
        }

        [HttpGet("{from}/{to}")]
        public async Task<ActionResult> GetRate(string from, string to, CancellationToken cancellationToken)
        {
            from = from.ToUpperInvariant();
            to = to.ToUpperInvariant();

            var stopwatch = Stopwatch.StartNew();
            try
            {
                var rate = await _exchangeRateProvider.GetRateAsync(from, to, cancellationToken);
                return Ok(new { From = from, To = to, Rate = rate, ElapsedMs = stopwatch.ElapsedMilliseconds });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, ex.Message);
            }
        }

    }
}
