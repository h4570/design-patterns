using System.Text;

namespace iterator.Bank
{
    public record Transaction(int Id, DateOnly Date, string Description, decimal Amount);

    /// <summary>
    /// One page returned by the bank. NextCursor is opaque - client only passes it back, null means last page.
    /// </summary>
    public record TransactionPage(IReadOnlyList<Transaction> Items, string NextCursor);

    /// <summary>
    /// Simulates external open banking API which returns account history page by page (25 transactions per request).
    /// Every request is slow and counted - banks often limit number of requests per day.
    /// </summary>
    public class FakeBankApi
    {

        public const int PageSize = 25;

        // Newest first, like in every banking app
        private readonly IReadOnlyList<Transaction> _transactions = Generate().OrderByDescending(c => c.Id).ToList();
        private int _requests;

        public int Requests => _requests;

        public async Task<TransactionPage> GetTransactionsAsync(string cursor, CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref _requests);
            await Task.Delay(80, cancellationToken); // network

            var offset = cursor == null ? 0 : int.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(cursor)));
            var items = _transactions.Skip(offset).Take(PageSize).ToList();
            var next = offset + PageSize < _transactions.Count
                ? Convert.ToBase64String(Encoding.UTF8.GetBytes((offset + PageSize).ToString()))
                : null;

            return new TransactionPage(items, next);
        }

        /// <summary>
        /// 230 transactions from November 2026, always the same (fixed seed).
        /// </summary>
        private static List<Transaction> Generate()
        {
            var random = new Random(2026);
            var shops = new (string Name, int Min, int Max)[]
            {
                ("Biedronka", 15, 260), ("Lidl", 20, 310), ("Orlen", 150, 360), ("Zabka", 8, 45),
                ("Allegro", 30, 900), ("Uber", 18, 75), ("Pharmacy", 12, 140), ("Restaurant", 45, 280)
            };

            var result = new List<Transaction>();
            const int count = 230;
            for (var i = 0; i < count; i++)
            {
                var date = new DateOnly(2026, 11, 1).AddDays(i * 30 / count);
                var (description, amount) = i switch
                {
                    0 => ("Rent - November", -3200m),
                    71 => ("Salary - ACME Sp. z o.o.", 9500m),
                    128 => ("Netflix", -60m),
                    163 => ("BLIK from Piotr - concert tickets", 240m),
                    _ => RandomPurchase(random, shops)
                };
                result.Add(new Transaction(i + 1, date, description, amount));
            }
            return result;
        }

        private static (string, decimal) RandomPurchase(Random random, (string Name, int Min, int Max)[] shops)
        {
            var shop = shops[random.Next(shops.Length)];
            return (shop.Name, -(random.Next(shop.Min * 100, shop.Max * 100) / 100m));
        }

    }
}
