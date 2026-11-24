using System.Runtime.CompilerServices;

namespace iterator.Bank
{

    /// <summary>
    /// Aggregate - looks like a simple collection of transactions.
    /// Client does not know about pages, cursors or HTTP requests, it just uses "await foreach".
    /// </summary>
    public class TransactionHistory : IAsyncEnumerable<Transaction>
    {

        private readonly FakeBankApi _api;

        public TransactionHistory(FakeBankApi api)
        {
            _api = api;
        }

        public IAsyncEnumerator<Transaction> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
            new PagedTransactionIterator(_api, cancellationToken);

        /// <summary>
        /// The same iterator written with "yield return" - compiler generates a class similar to PagedTransactionIterator.
        /// </summary>
        public static async IAsyncEnumerable<Transaction> ReadAll(FakeBankApi api,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            string cursor = null;
            do
            {
                var page = await api.GetTransactionsAsync(cursor, cancellationToken);
                foreach (var transaction in page.Items)
                    yield return transaction;
                cursor = page.NextCursor;
            } while (cursor != null);
        }

    }

    /// <summary>
    /// Iterator - remembers where it is (current page, position, cursor of the next page)
    /// and downloads the next page only when the current one runs out.
    /// </summary>
    public class PagedTransactionIterator : IAsyncEnumerator<Transaction>
    {

        private readonly FakeBankApi _api;
        private readonly CancellationToken _cancellationToken;

        private IReadOnlyList<Transaction> _page = Array.Empty<Transaction>();
        private int _index = -1;
        private string _nextCursor;
        private bool _lastPageLoaded;

        public PagedTransactionIterator(FakeBankApi api, CancellationToken cancellationToken)
        {
            _api = api;
            _cancellationToken = cancellationToken;
        }

        public Transaction Current => _page[_index];

        public async ValueTask<bool> MoveNextAsync()
        {
            _index++;
            // "while", not "if" - bank can return an empty page with a cursor to the next one
            while (_index >= _page.Count)
            {
                if (_lastPageLoaded) return false;

                var page = await _api.GetTransactionsAsync(_nextCursor, _cancellationToken);
                _page = page.Items;
                _index = 0;
                _nextCursor = page.NextCursor;
                _lastPageLoaded = page.NextCursor == null;
            }
            return true;
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    }
}
