# Iterator pattern

In this example iterator pattern was used to read **bank account history from a paginated open banking API**.  
Bank returns 25 transactions per request with an opaque cursor to the next page, every request is slow and banks limit how many of them you can make.  
`TransactionHistory` hides all of that - for the client it is just a collection used in `await foreach`.  
`PagedTransactionIterator` remembers the current page and position, and downloads the next page **only when the current one runs out**.  
Thanks to that "latest 5 transactions" widget makes 1 request, and searching stops downloading as soon as the result is found.  
To test this example, run "**/statement/demo**" endpoint. `GET /statement` streams the whole month, `GET /statement/summary` sums it up.  
Please look on **TransactionHistory.cs** to get better view of this implementation.

## Participants

- `IAsyncEnumerable<T>` / `IAsyncEnumerator<T>` - **aggregate** and **iterator** interfaces, built into .NET
- `TransactionHistory` - **concrete aggregate**, creates a new iterator for every `await foreach`
- `PagedTransactionIterator` - **concrete iterator**, keeps the traversal state (page, index, cursor)
- `FakeBankApi` - external paginated API, hidden from the client
- `StatementController` - **client**, iterates without knowing about pages

### Iterator implementation
```csharp
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
```

### The same with `yield return`
In C# you rarely write the iterator class by hand - the compiler generates a very similar state machine from this method:
```csharp
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
```

### Client code
```csharp
var result = new List<Transaction>();
await foreach (var transaction in history)
{
    result.Add(transaction);
    if (result.Count == 5) break;   // iterator stops here, next pages are never downloaded
}
```
Returning `IAsyncEnumerable<T>` from a controller makes ASP.NET Core stream the JSON array while pages are still being downloaded:
```csharp
[HttpGet]
public IAsyncEnumerable<Transaction> GetAll(CancellationToken cancellationToken) =>
    TransactionHistory.ReadAll(_api, cancellationToken);
```

### Result (`/statement/demo`, shortened)
| Use case | Requests to bank | Time | Result |
|---|---|---|---|
| Latest 5 transactions | 1 | ~90 ms | Biedronka -154.29, Pharmacy -29.63, ... |
| Latest income | 3 | ~270 ms | BLIK from Piotr - concert tickets, +240 |
| Monthly summary (230 transactions) | 10 | ~950 ms | income 9740, expenses -41796.89 |

Client code is the same `await foreach` in all three cases - only the iterator knows that the bank has pages.

## Where else you can find it

Every `foreach` in C# uses an iterator (`IEnumerable<T>.GetEnumerator()`), and LINQ is a set of iterators chained together.  
Azure SDK's `AsyncPageable<T>` and EF Core's `AsAsyncEnumerable()` hide pages and database cursors exactly like this example.
