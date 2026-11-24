using flyweight.Seating;
using Microsoft.AspNetCore.Mvc;

namespace flyweight.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StadiumController : ControllerBase
    {

        private readonly Stadium _stadium;
        private readonly SeatCategoryFactory _categories;

        public StadiumController(Stadium stadium, SeatCategoryFactory categories)
        {
            _stadium = stadium;
            _categories = categories;
        }

        /// <summary>
        /// Builds the whole seat map twice - with and without flyweight - and compares allocated memory.
        /// </summary>
        [HttpGet("memory")]
        public ActionResult<object> CompareMemory()
        {
            // Memory which stays in use after building (garbage is collected first)
            var before = GC.GetTotalMemory(forceFullCollection: true);
            var naiveSeats = StadiumLayout.Build((sector, row, number, category) =>
                new NaiveSeat(sector, row, number, SeatCategoryCatalog.Load(category)));
            var naiveBytes = GC.GetTotalMemory(forceFullCollection: true) - before;
            var naiveCount = naiveSeats.Count;
            naiveSeats = null;

            var factory = new SeatCategoryFactory();
            before = GC.GetTotalMemory(forceFullCollection: true);
            var flyweightSeats = StadiumLayout.Build((sector, row, number, category) =>
                new Seat(sector, row, number, factory.Get(category)));
            var flyweightBytes = GC.GetTotalMemory(forceFullCollection: true) - before;

            return Ok(new
            {
                Seats = flyweightSeats.Count,
                WithoutFlyweight = new { CategoryObjects = naiveCount, MemoryMb = ToMb(naiveBytes) },
                WithFlyweight = new { CategoryObjects = factory.CreatedCount, MemoryMb = ToMb(flyweightBytes) },
                MemorySavedPercent = Math.Round(100 - 100.0 * flyweightBytes / naiveBytes, 1)
            });
        }

        /// <summary>
        /// Shared categories and how many seats point to each of them.
        /// </summary>
        [HttpGet("categories")]
        public ActionResult<object> GetCategories() => Ok(_stadium.AllSeats
            .GroupBy(c => c.Category) // grouping by reference - works only because instances are shared
            .Select(c => new
            {
                c.Key.Name,
                c.Key.Price,
                c.Key.ColorHex,
                c.Key.Perks,
                SeatsSharingThisInstance = c.Count(),
                Sold = c.Count(s => s.IsSold)
            }));

        [HttpGet("sectors/{sector}/rows/{row:int}")]
        public ActionResult<object> GetRow(string sector, int row)
        {
            var seats = _stadium.GetRow(sector, row);
            if (seats.Count == 0) return NotFound();
            return Ok(seats.Select(ToDto));
        }

        [HttpPost("sectors/{sector}/rows/{row:int}/seats/{number:int}/buy")]
        public ActionResult<object> Buy(string sector, int row, int number)
        {
            try
            {
                return Ok(ToDto(_stadium.Buy(sector, row, number)));
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        }

        private static object ToDto(Seat seat) => new
        {
            seat.Sector,
            seat.Row,
            seat.Number,
            seat.IsSold,
            Category = seat.Category.Name,
            seat.Category.Price,
            seat.Category.ColorHex
        };

        private static double ToMb(long bytes) => Math.Round(bytes / 1024.0 / 1024.0, 2);

    }
}
