namespace flyweight.Seating
{
    /// <summary>
    /// Context - extrinsic (unique) state of a seat + reference to the shared flyweight.
    /// </summary>
    public class Seat
    {

        public string Sector { get; }
        public int Row { get; }
        public int Number { get; }
        public bool IsSold { get; set; }
        public SeatCategory Category { get; }

        public Seat(string sector, int row, int number, SeatCategory category)
        {
            Sector = sector;
            Row = row;
            Number = number;
            Category = category;
        }

    }

    /// <summary>
    /// Seat WITHOUT flyweight - every seat holds its own copy of category data. Used only for comparison.
    /// </summary>
    public class NaiveSeat
    {

        public string Sector { get; }
        public int Row { get; }
        public int Number { get; }
        public bool IsSold { get; set; }

        public string CategoryName { get; }
        public decimal Price { get; }
        public string ColorHex { get; }
        public string Description { get; }
        public IReadOnlyList<string> Perks { get; }
        public string IconSvg { get; }

        public NaiveSeat(string sector, int row, int number, SeatCategory category)
        {
            Sector = sector;
            Row = row;
            Number = number;
            CategoryName = category.Name;
            Price = category.Price;
            ColorHex = category.ColorHex;
            Description = category.Description;
            Perks = category.Perks;
            IconSvg = category.IconSvg;
        }

    }
}
