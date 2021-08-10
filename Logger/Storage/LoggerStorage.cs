namespace SCLogger
{
    public class LoggerStorage// : DynamicLogger
    {
        public static LoggerStorage Instance { get; private set; } = new LoggerStorage();

        /// <summary>
        /// Инициализация обработчика лог сообщений
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">Название по которому можно обратиться к обработчику через LoggerStorage.Instance.<name>...</param>
        /// <param name="fname">Префикс имени файла <fname> <datetime>.log</param>
        /// <param name="path">Путь к файлам логирования</param>
        /// <param name="delay">Время перерыва между выгрузками лог сообщений в файл</param>
        /// <returns></returns>
        public static T InitializeLogger<T>(string name, string fname, string path, int delay)
            where T : ILogger, new()
        {
            var logger = new T();
            //Instance.dictionary[name] = logger;
            logger.Initialize(fname, path, delay);

            return logger;
        }

        //public static void DestroyLogger(string name)
        //{
        //    var instance = Instance.dictionary[name];

        //    if (instance != null)
        //    {
        //        if (instance is BaseLogger bl)
        //        {
        //            bl.Dispose();
        //        }
        //        Instance.dictionary[name] = null;
        //    }
        //}
    }
}
