# Proxy pattern

In this example proxy pattern was used to guard **HR documents** (payslips, contracts, medical certificates) stored in an encrypted cloud storage.  
`CloudDocumentStorage` only stores files - it knows nothing about users. Connecting to it is expensive (~1.5 s: storage account + encryption keys).  
`DocumentStorageProxy` implements the same `IDocumentStorage` interface and sits in front of it, so the controller does not even know it talks to a proxy.  
The proxy combines three classic kinds of proxy:
- **virtual proxy** - real storage is created lazily, on the first request which really needs it
- **protection proxy** - employees see only their own documents, only HR can see everything and delete
- **logging proxy** - every access to personal data lands in the audit log (GDPR)

To test this example, run "**/documents?user=anna**", "**/documents/1?user=piotr**" (forbidden), "**/documents?user=kasia**" (HR) and "**/documents/storage-status**".  
Users are taken from `?user=` parameter to keep the example simple - available users: *anna*, *piotr* (employees), *kasia* (HR).  
Please look on **DocumentStorageProxy.cs** and **Program.cs** to get better view of this implementation.

### Proxy implementation
```csharp
public Document Get(int id)
{
    EnsureAuthenticated($"read document {id}");
    var document = _storage.Value.Get(id);            // Lazy<CloudDocumentStorage> - connects on first use
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
```

### Wiring
```csharp
builder.Services.AddSingleton<CloudDocumentStorage>();
builder.Services.AddSingleton(sp => new Lazy<CloudDocumentStorage>(sp.GetRequiredService<CloudDocumentStorage>));

// Clients asking for IDocumentStorage get the proxy, never the real storage
builder.Services.AddScoped<IDocumentStorage, DocumentStorageProxy>();
```

### Client code
```csharp
// Controller depends on the interface only - it has no idea that it gets a proxy
public DocumentsController(IDocumentStorage documents) { _documents = documents; }

[HttpGet("{id:int}")]
public ActionResult<Document> Get(int id) => Execute(() => _documents.Get(id));
```

### Result
```
GET    /documents/storage-status      200   {"realStorageCreated":false}
GET    /documents                     401   You have to be logged in
DELETE /documents/1?user=anna         403   anna is not allowed to delete document 1
GET    /documents/storage-status      200   {"realStorageCreated":false}     <- still not connected
GET    /documents?user=anna           200   1.52 s   [Payslip 10/2026, Employment contract]
GET    /documents?user=piotr          200   0.002 s  [Payslip 10/2026, Medical certificate]
GET    /documents/1?user=piotr        403   piotr is not allowed to read document 1 of anna
GET    /documents?user=kasia          200   all 5 documents
DELETE /documents/4?user=kasia        200   Document 4 deleted
```
Audit log:
```
AUDIT anonymous user tried to list
AUDIT DENIED anna tried to delete document 1
Connecting to cloud storage and downloading encryption keys...
AUDIT anna listed 2 documents
AUDIT DENIED piotr tried to read document 1 of anna
AUDIT kasia deleted document 4
```

## Proxy vs Decorator

Both wrap an object with the same interface, so in code they look almost identical.  
The difference is in intent: **decorator adds behaviour** and is composed by the client (see `decorator` project - retry, cache, logging stacked in any order),  
**proxy controls access** to the object and usually manages its lifetime - here the client cannot even get the real storage without going through the proxy.
