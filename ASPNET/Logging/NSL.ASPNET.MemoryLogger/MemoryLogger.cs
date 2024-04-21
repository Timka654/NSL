using System.Text;

namespace NSL.ASPNET.MemoryLogger
{
    public class MemoryLogger : ILogger
    {
        private readonly string name;
        private readonly MemoryLoggerProvider loggerProvider;

        public MemoryLogger(string name, MemoryLoggerProvider loggerProvider)
        {
            this.name = name;
            this.loggerProvider = loggerProvider;
        }

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
            => default!;

        public bool IsEnabled(LogLevel logLevel)
            => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            StringBuilder sb = new StringBuilder();

            loggerProvider.EnqueueLog(new MemoryLogRecordModel(logLevel, eventId, name, formatter(state, exception)));
        }
    }
}
