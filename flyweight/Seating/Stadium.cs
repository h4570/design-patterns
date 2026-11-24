namespace flyweight.Seating
{
    /// <summary>
    /// Seat map of the whole stadium, kept in memory for fast seat availability checks.
    /// </summary>
    public class Stadium
    {

        private readonly Dictionary<(string Sector, int Row, int Number), Seat> _seats;
        private readonly object _lock = new();

        public Stadium(SeatCategoryFactory categories)
        {
            var random = new Random(2026);
            var seats = StadiumLayout.Build((sector, row, number, category) =>
                new Seat(sector, row, number, categories.Get(category)) { IsSold = random.NextDouble() < 0.35 });
            _seats = seats.ToDictionary(c => (c.Sector, c.Row, c.Number));
        }

        public int SeatsCount => _seats.Count;

        public IEnumerable<Seat> AllSeats => _seats.Values;

        public IReadOnlyList<Seat> GetRow(string sector, int row) => _seats.Values
            .Where(c => c.Sector == sector.ToUpperInvariant() && c.Row == row)
            .OrderBy(c => c.Number)
            .ToList();

        /// <exception cref="KeyNotFoundException">Seat does not exist</exception>
        /// <exception cref="InvalidOperationException">Seat is already sold</exception>
        public Seat Buy(string sector, int row, int number)
        {
            lock (_lock)
            {
                if (!_seats.TryGetValue((sector.ToUpperInvariant(), row, number), out var seat))
                    throw new KeyNotFoundException($"Seat {sector}/{row}/{number} does not exist");
                if (seat.IsSold)
                    throw new InvalidOperationException($"Seat {sector}/{row}/{number} is already sold");
                // Only extrinsic state changes - shared category stays untouched
                seat.IsSold = true;
                return seat;
            }
        }

    }
}
