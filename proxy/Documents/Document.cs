namespace proxy.Documents
{
    public record Document(int Id, string Owner, string Title, string Content);

    public record DocumentInfo(int Id, string Owner, string Title);

    public interface IDocumentStorage
    {
        IReadOnlyList<DocumentInfo> List();

        /// <exception cref="KeyNotFoundException">Document does not exist</exception>
        Document Get(int id);

        /// <exception cref="KeyNotFoundException">Document does not exist</exception>
        void Delete(int id);
    }
}
