using System.Text;

namespace flyweight.Seating
{
    /// <summary>
    /// Source of category data. In real life it would be a database / CMS - every call returns brand new objects,
    /// just like an ORM materializing a row.
    /// </summary>
    public static class SeatCategoryCatalog
    {

        public const string Vip = "VIP";
        public const string Premium = "Premium";
        public const string Standard = "Standard";

        public static SeatCategory Load(string name) => name switch
        {
            Vip => new SeatCategory(Vip, 899m, "#d4af37",
                $"{Vip} lounge seat with the best view of the pitch",
                new List<string> { "Lounge access", "Free catering", "Parking spot", "Padded seat" },
                BuildIconSvg("#d4af37", "crown")),
            Premium => new SeatCategory(Premium, 349m, "#3b82f6",
                $"{Premium} seat in the lower tier, close to the action",
                new List<string> { "Padded seat", "Fast track entrance" },
                BuildIconSvg("#3b82f6", "star")),
            Standard => new SeatCategory(Standard, 149m, "#9ca3af",
                $"{Standard} seat in the upper tier",
                new List<string>(),
                BuildIconSvg("#9ca3af", "seat")),
            _ => throw new ArgumentException($"Unknown seat category '{name}'", nameof(name))
        };

        private static string BuildIconSvg(string color, string shape)
        {
            // ~1 KB of markup, like a real icon exported from a design tool
            var svg = new StringBuilder($"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 64 64\" data-shape=\"{shape}\">");
            for (var i = 0; i < 12; i++)
                svg.Append($"<path fill=\"{color}\" d=\"M{i * 5} {64 - i * 5} L{32 + i} {i * 2} L{64 - i * 5} {64 - i * 5} Z\"/>");
            return svg.Append("</svg>").ToString();
        }

    }
}
