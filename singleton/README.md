# Singleton pattern

In this example, Singleton was used to implement thread safe file *LoggerService* in Web API.  
Logs are collected in memory (`ConcurrentQueue`) and saved to `logs/log.txt` in batches every 5 seconds, so requests never wait for disk.  
To test this example, run "**/logger-test**" endpoint a few times (or stress test it), then "**/logger-test/flush**" to save logs immediately and see the last lines.  
Please look on **LoggerService.cs** and **LoggerTestController.cs** to get better view of this implementation.

### Singleton implementation (double-checked locking)
```csharp
// volatile - other threads must never see a reference to a not fully constructed object
private static volatile LoggerService _instance;
private static readonly object _instanceLock = new();

public static LoggerService GetInstance()
{
    // 3 threads are using this method at the same time
    if (_instance == null)
        // All 3 threads goes there
        lock (_instanceLock)
            // One of them is using lock and goes there, other waits before lock.
            if (_instance == null)
                // One of threads is creating instance, other 2 threads after wait, will not reach this place
                _instance = new LoggerService();
    return _instance;
}
```
In modern .NET the same can be achieved with `private static readonly Lazy<LoggerService> _instance = new(() => new LoggerService());`.  
In ASP.NET Core you usually just register a class with `AddSingleton<T>()` - here DI container is fed with our own instance:
```csharp
builder.Services.AddSingleton(_ => LoggerService.GetInstance());
```
Thanks to that, the container calls `Dispose()` on shutdown and logs which are still in memory are saved.

### Saving logs
```csharp
private void SaveLogs(bool waitForPreviousSave)
{
    // Timer ticks can overlap when saving is slow. Next tick will simply take what is left.
    if (waitForPreviousSave) Monitor.Enter(_saveLock);
    else if (!Monitor.TryEnter(_saveLock)) return;

    try
    {
        // TryDequeue instead of CopyTo() + Clear() - logs added in between are not lost
        while (_logs.TryDequeue(out var log))
            _unsaved.Add(log);
        if (_unsaved.Count == 0) return;

        Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
        File.AppendAllLines(FilePath, _unsaved);
        _unsaved.Clear();
    }
    catch (IOException ex)
    {
        // Never throw from timer callback - it would kill the whole application.
        // Logs stay in _unsaved and will be saved with next tick.
    }
    finally
    {
        Monitor.Exit(_saveLock);
    }
}
```

### Client code
```csharp
[HttpGet]
public ActionResult Get()
{
    var currentSecondInString = DateTime.Now.Second.ToString();
    _loggerService.Add(currentSecondInString);
    return Ok("done ...");
}
```

### Stress test
100 requests, 20 in parallel:
```bash
seq 1 100 | xargs -P 20 -I{} curl -s -o /dev/null http://localhost:5000/logger-test
curl http://localhost:5000/logger-test/flush
```
```
2026-11-24 18:55:39.055 39
2026-11-24 18:55:39.096 39
2026-11-24 18:55:39.131 39
...
```
Every single request ends up in the file - nothing is lost or written twice.
