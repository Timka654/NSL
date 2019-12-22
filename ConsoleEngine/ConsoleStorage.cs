using SocketCore.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ConsoleEngine
{
    /// <summary>
    /// Хранилище конфигураций
    /// </summary>
    public class ConsoleStorage<T>
        where T : INetworkClient
    {
        /// <summary>
        /// Список данных конфигураций
        /// </summary>
        protected ConcurrentDictionary<string, Func<T, string[],string>> command_map;

        internal ConsoleStorage()
        {
            command_map = new ConcurrentDictionary<string, Func<T, string[],string>>();
        }

        /// <summary>
        /// Добавить значение конфигурации
        /// </summary>
        /// <param name="command">Данные конфигурации</param>
        public virtual void AddCommand(string command, Func<T, string[],string> commandArgs)
        {
            command_map.TryAdd(command.ToLower(), commandArgs);
        }

        /// <summary>
        /// Удалить значение конфигурации
        /// </summary>
        /// <param name="name">Полный путь</param>
        public virtual void RemoveCommand(string command)
        {
            command_map.TryRemove(command.ToLower(),out var r);
        }

        public virtual Func<T, string[], string> GetCommand(string command)
        {
            command_map.TryGetValue(command.ToLower(), out var r);

            return r;
        }
    }
}
