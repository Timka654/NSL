namespace Logger
{
    public interface ILogger
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fname">название файла (к прим. "file")</param>
        /// <param name="path">Путь к директории с файлами лога</param>
        /// <param name="delay">Время между выводами данных с кэша</param>
        void Initialize(string fname, string path, int delay);

        void Append(LoggerLevel level, string text);

        void SetConsoleOutput(bool allow);

        void SetUnhandledExCatch(bool allow);
    }
}
