namespace flyweight.Seating
{
    /// <summary>
    /// Flyweight - intrinsic (shared) state of a seat.
    /// Thousands of seats have exactly the same category data, so one immutable instance is shared by all of them.
    /// </summary>
    public sealed class SeatCategory
    {

        public string Name { get; }
        public decimal Price { get; }
        public string ColorHex { get; }
        public string Description { get; }
        public IReadOnlyList<string> Perks { get; }
        /// <summary>Icon rendered on the seat map - the heaviest part of the category.</summary>
        public string IconSvg { get; }

        public SeatCategory(string name, decimal price, string colorHex, string description, IReadOnlyList<string> perks, string iconSvg)
        {
            Name = name;
            Price = price;
            ColorHex = colorHex;
            Description = description;
            Perks = perks;
            IconSvg = iconSvg;
        }

    }
}
