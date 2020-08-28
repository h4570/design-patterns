using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace singleton.Services
{
    public class LoggerService
    {

        public readonly Timer SaveLogsTimer;

        private static LoggerService _instance;
        private static readonly object _lock = new object();
        private bool _loggerIsBusy;
        private int _saveLogsFailCount;
        private const int LOG_FAIL_COUNT = 5;

        private LoggerService()
        {
            _saveLogsFailCount = 0;
            _logs = new ConcurrentQueue<string>();
            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromMinutes(1);
            SaveLogsTimer = new Timer((e) => SaveLogs(), null, startTimeSpan, periodTimeSpan);
        }

        public static LoggerService GetInstance()
        {
            // Description: 
            // 3 threads are using this constructor at the same time
            if (_instance == null)
                // All 3 threads goes there
                lock (_lock)
                    // One of them is using lock and goes there, other waits before lock.
                    if (_instance == null)
                        // One of threads is creating instance, other 2 threads after wait, will not reach this place
                        _instance = new LoggerService();
            return _instance;
        }

        private readonly ConcurrentQueue<string> _logs;

        public void Add(string log) { _logs.Enqueue(log); }

        private void SaveLogs()
        {
            if (_saveLogsFailCount > LOG_FAIL_COUNT) throw new Exception($"Logger failed to save logs {LOG_FAIL_COUNT} times");
            if (!_loggerIsBusy)
            {
                _saveLogsFailCount = 0;
                var count = _logs.Count;
                if (count > 0)
                {
                    _loggerIsBusy = true;
                    var logsToSave = new string[count];
                    _logs.CopyTo(logsToSave, 0);
                    _logs.Clear();
                    File.AppendAllLines(Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\Desktop\log.txt"), logsToSave);
                    _loggerIsBusy = false;
                }
            }
            else _saveLogsFailCount++;
        }

    }

}
