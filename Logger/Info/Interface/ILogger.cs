﻿using SocketCore.Utils.Logger;
using Utils;

namespace SCLogger
{
    public interface ILogger : IBasicLogger
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fname">название файла (к прим. "file")</param>
        /// <param name="path">Путь к директории с файлами лога</param>
        /// <param name="delay">Время между выводами данных с кэша</param>
        void Initialize(string fname, string path, int delay);

        void SetConsoleOutput(bool allow);

        void SetUnhandledExCatch(bool allow);
    }
}
