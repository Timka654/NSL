using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.ASPNET.MemoryLogger
{
    public sealed class MemoryLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, MemoryLogger> _loggers =
            new(StringComparer.OrdinalIgnoreCase);

        internal static TimeSpan DefaultValidTime = Timeout.InfiniteTimeSpan;

        internal const int DefaultMaxLogCount = 10_000;

        TimeSpan validTime { get; } = DefaultValidTime;

        int maxLogCount { get; } = DefaultMaxLogCount;

        long logId = 0;

        public MemoryLoggerProvider(int maxLogCount = DefaultMaxLogCount) : this(DefaultValidTime, maxLogCount)
        {
        }

        public MemoryLoggerProvider(TimeSpan validTime, int maxLogCount = DefaultMaxLogCount)
        {
            this.validTime = validTime;
            this.maxLogCount = maxLogCount;
        }

        public ILogger CreateLogger(string categoryName) =>
            _loggers.GetOrAdd(categoryName, name => new MemoryLogger(name, this));

        private ConcurrentQueue<MemoryLogRecordModel> logs = new();

        private AutoResetEvent _clearLocker = new AutoResetEvent(true);

        internal void EnqueueLog(MemoryLogRecordModel record)
        {
            logs.Enqueue(record);

            if (_clearLocker.WaitOne(0))
                ClearLogs();
        }

        private async void ClearLogs()
        {
            try
            {
                await Task.Run(() =>
                {
                    if (maxLogCount > 0 && logs.Count > maxLogCount)
                    {
                        while (logs.Count > maxLogCount) logs.TryDequeue(out _);
                    }

                    if (!validTime.Equals(Timeout.InfiniteTimeSpan))
                    {
                        var _validTime = DateTime.UtcNow.Add(-validTime);

                        while (logs.TryPeek(out var log) && log.CreateTime < _validTime) logs.TryDequeue(out _);
                    }
                });

            }
            catch (TaskCanceledException) { }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                _clearLocker.Set();
            }
        }

        public IEnumerable<MemoryLogRecordModel> GetLogs()
            => logs.ToArray();

        public IEnumerable<string> GetTextLogs()
            => logs.Select(x => x.ToString()).ToArray();

        public void Dispose()
            => _loggers.Clear();
    }
}