using System.Collections.Concurrent;

namespace proxy.Documents
{
    /// <summary>
    /// Real subject - encrypted cloud storage with HR documents.
    /// Creating it is expensive: connection to the storage account and downloading encryption keys from the vault.
    /// It knows nothing about users and permissions.
    /// </summary>
    public class CloudDocumentStorage : IDocumentStorage
    {

        public static readonly TimeSpan ConnectionTime = TimeSpan.FromMilliseconds(1500);

        private readonly ConcurrentDictionary<int, Document> _documents;

        public CloudDocumentStorage(ILogger<CloudDocumentStorage> logger)
        {
            logger.LogWarning("Connecting to cloud storage and downloading encryption keys...");
            Thread.Sleep(ConnectionTime);
            logger.LogWarning("Connected to cloud storage");

            _documents = new ConcurrentDictionary<int, Document>(new[]
            {
                new Document(1, "anna", "Payslip 10/2026", "Gross: 12 500 PLN, net: 8 950 PLN"),
                new Document(2, "anna", "Employment contract", "Senior developer, full time, since 2021-03-01"),
                new Document(3, "piotr", "Payslip 10/2026", "Gross: 9 800 PLN, net: 7 090 PLN"),
                new Document(4, "piotr", "Medical certificate", "Sick leave 2026-10-12 - 2026-10-16"),
                new Document(5, "kasia", "Payslip 10/2026", "Gross: 11 000 PLN, net: 7 900 PLN"),
            }.ToDictionary(c => c.Id));
        }

        public IReadOnlyList<DocumentInfo> List() => _documents.Values
            .OrderBy(c => c.Id)
            .Select(c => new DocumentInfo(c.Id, c.Owner, c.Title))
            .ToList();

        public Document Get(int id) => _documents.TryGetValue(id, out var document)
            ? document
            : throw new KeyNotFoundException($"Document {id} does not exist");

        public void Delete(int id)
        {
            if (!_documents.TryRemove(id, out _))
                throw new KeyNotFoundException($"Document {id} does not exist");
        }

    }
}
