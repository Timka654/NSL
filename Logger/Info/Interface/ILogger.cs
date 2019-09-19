using System;
using System.Collections.Generic;
using System.Text;

namespace Logger
{
    public interface ILogger
    {
        void Initialize(string fname, string path, int delay);

        void Append(LoggerLevel level, string text);

        void SetConsoleOutput(bool allow);

        void SetUnhandledExCatch(bool allow);
    }
}
