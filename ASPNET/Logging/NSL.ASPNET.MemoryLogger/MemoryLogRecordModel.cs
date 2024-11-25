using Microsoft.Extensions.Logging;
using System.Text;
using System;

namespace NSL.ASPNET.MemoryLogger
{
    public class MemoryLogRecordModel
    {
        public MemoryLogRecordModel(LogLevel logLevel, EventId eventId, string categoryName, string message)
        {
            LogLevel = logLevel;
            EventId = eventId;
            CategoryName = categoryName;
            Message = message;
            CreateTime = DateTime.UtcNow;
        }

        public LogLevel LogLevel { get; }

        public EventId EventId { get; }

        public string CategoryName { get; }

        public string Message { get; }

        public DateTime CreateTime { get; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append($"{CreateTime} = [{EventId.Id,2}: {LogLevel,-12}]");
            sb.Append($"     {CategoryName} - {Message}");

            return sb.ToString();
        }
    }
}
