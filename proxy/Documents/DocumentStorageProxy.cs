using proxy.Users;

namespace proxy.Documents
{
    public class NotAuthenticatedException : Exception
    {
        public NotAuthenticatedException() : base("You have to be logged in") { }
    }

    /// <summary>
    /// Proxy - has the same interface as CloudDocumentStorage, so clients do not know they talk to a proxy.
    /// It combines three classic kinds of proxy:
    /// - virtual proxy    - real storage is created lazily, on first request which really needs it
    /// - protection proxy - checks permissions of the current user
    /// - logging proxy    - writes audit log of every access to personal data (GDPR)
    /// </summary>
    public class DocumentStorageProxy : IDocumentStorage
    {

        private readonly Lazy<CloudDocumentStorage> _storage;
        private readonly CurrentUser _user;
        private readonly ILogger<DocumentStorageProxy> _audit;

        public DocumentStorageProxy(Lazy<CloudDocumentStorage> storage, CurrentUser user, ILogger<DocumentStorageProxy> audit)
        {
            _storage = storage;
            _user = user;
            _audit = audit;
        }

        public IReadOnlyList<DocumentInfo> List()
        {
            EnsureAuthenticated("list");
            var documents = _storage.Value.List();
            // Employees see only their own documents, HR sees everything
            var visible = _user.Role == Role.Hr
                ? documents
                : documents.Where(c => c.Owner == _user.Name).ToList();
            _audit.LogInformation("AUDIT {User} listed {Count} documents", _user.Name, visible.Count);
            return visible;
        }

        public Document Get(int id)
        {
            EnsureAuthenticated($"read document {id}");
            var document = _storage.Value.Get(id);
            if (_user.Role != Role.Hr && document.Owner != _user.Name)
                Deny($"read document {id} of {document.Owner}");
            _audit.LogInformation("AUDIT {User} read document {Id} of {Owner}", _user.Name, id, document.Owner);
            return document;
        }

        public void Delete(int id)
        {
            EnsureAuthenticated($"delete document {id}");
            // Checked before touching the real storage - forbidden request does not even open the connection
            if (_user.Role != Role.Hr)
                Deny($"delete document {id}");
            _storage.Value.Delete(id);
            _audit.LogWarning("AUDIT {User} deleted document {Id}", _user.Name, id);
        }

        private void EnsureAuthenticated(string action)
        {
            if (_user.Role != Role.Anonymous) return;
            _audit.LogWarning("AUDIT anonymous user tried to {Action}", action);
            throw new NotAuthenticatedException();
        }

        private void Deny(string action)
        {
            _audit.LogWarning("AUDIT DENIED {User} tried to {Action}", _user.Name, action);
            throw new UnauthorizedAccessException($"{_user.Name} is not allowed to {action}");
        }

    }
}
