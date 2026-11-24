using System.Collections.Concurrent;

namespace singleton.Services
{
    /// <summary>
    /// Thread safe file logger. Logs are collected in memory and written to the file in batches by a timer,
    /// so requests never wait for disk I/O.
    /// </summary>
    public sealed class LoggerService : IDisposable
    {

        public static readonly TimeSpan SaveInterval = TimeSpan.FromSeconds(5);
        public string FilePath { get; }

        // volatile - other threads must never see a reference to a not fully constructed object
        private static volatile LoggerService _instance;
        private static readonly object _instanceLock = new();

        private readonly ConcurrentQueue<string> _logs = new();
        // Logs taken from the queue, but not saved yet (e.g. file was locked). Accessed only under _saveLock.
        private readonly List<string> _unsaved = new();
        private readonly object _saveLock = new();
        private readonly Timer _saveLogsTimer;
        private bool _disposed;

        private LoggerService()
        {
            FilePath = Path.Combine(Directory.GetCurrentDirectory(), "logs", "log.txt");
            _saveLogsTimer = new Timer(_ => SaveLogs(waitForPreviousSave: false), null, SaveInterval, SaveInterval);
        }

        public static LoggerService GetInstance()
        {
            // Description:
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

        public void Add(string log) => _logs.Enqueue($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} {log}");

        /// <summary>
        /// Writes all pending logs to the file right now.
        /// </summary>
        public void Flush() => SaveLogs(waitForPreviousSave: true);

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
                Console.Error.WriteLine($"LoggerService: cannot save {_unsaved.Count} logs, will retry. {ex.Message}");
            }
            finally
            {
                Monitor.Exit(_saveLock);
            }
        }

        /// <summary>
        /// Called by DI container on application shutdown - saves what is still in memory.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _saveLogsTimer.Dispose();
            Flush();
        }

    }

}
