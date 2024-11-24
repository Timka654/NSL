using NSL.SocketCore.Utils.Logger.Enums;
using System;

namespace NSL.Logger.Info
{
    public class LogMessageInfo
    {
        public string Text { get; set; }

        public DateTime Now { get; set; }

        public LoggerLevel Level { get; set; }

        private string Temp;

        public override string ToString()
        {
            if (Temp == null)
            {
                Temp = $"[{Level.ToString()}] - {DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}: {Text}";
                Text = null;
            }

            return Temp;
        }
    }
}
