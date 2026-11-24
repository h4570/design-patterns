namespace flyweight.Seating
{
    /// <summary>
    /// 20 sectors (A-T) x 40 rows x 50 seats = 40 000 seats.
    /// </summary>
    public static class StadiumLayout
    {

        public static readonly string[] Sectors = Enumerable.Range('A', 20).Select(c => ((char)c).ToString()).ToArray();
        public const int RowsPerSector = 40;
        public const int SeatsPerRow = 50;

        public static string CategoryForRow(int row) => row switch
        {
            <= 5 => SeatCategoryCatalog.Vip,
            <= 20 => SeatCategoryCatalog.Premium,
            _ => SeatCategoryCatalog.Standard
        };

        /// <summary>
        /// Calls <paramref name="create"/> for every seat position in the stadium.
        /// </summary>
        public static List<T> Build<T>(Func<string, int, int, string, T> create)
        {
            var seats = new List<T>(Sectors.Length * RowsPerSector * SeatsPerRow);
            foreach (var sector in Sectors)
                for (var row = 1; row <= RowsPerSector; row++)
                    for (var number = 1; number <= SeatsPerRow; number++)
                        seats.Add(create(sector, row, number, CategoryForRow(row)));
            return seats;
        }

    }
}
