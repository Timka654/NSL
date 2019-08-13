using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SocketServer.Utils;

namespace gls.Data.Basic.Storages
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

        public ConsoleStorage()
        {
            command_map = new ConcurrentDictionary<string, Func<T, string[],string>>();
        }

        /// <summary>
        /// Добавить значение конфигурации
        /// </summary>
        /// <param name="command">Данные конфигурации</param>
        public virtual void AddValue(string command, Func<T, string[],string> commandArgs)
        {
            command_map.TryAdd(command.ToLower(), commandArgs);
        }

        /// <summary>
        /// Удалить значение конфигурации
        /// </summary>
        /// <param name="name">Полный путь</param>
        public virtual void RemoveValue(string name)
        {
            command_map.Remove(name.ToLower(),out var r);
        }

        /// <summary>
        /// Получить значение конфигурации
        /// </summary>
        /// <param name="name">Полный путь</param>
        /// <returns></returns>
        public virtual Func<T, string[],string> GetValue(string name)
        {
            command_map.TryGetValue(name.ToLower(), out Func<T, string[],string> c);
            return c;
        }

        /// <summary>
        /// Потокобезопасный Linq Where
        /// </summary>
        /// <param name="predicate">Выражение для сравнения</param>
        /// <returns>Данные прошедшие фильтрацию</returns>
        public virtual IEnumerable<Func<T, string[],string>> ConfigWhere(Func<Func<T, string[],string>, bool> predicate)
        {
            return command_map.Values.Where(predicate);
        }

        /// <summary>
        /// Потокобезопасный Linq Exists
        /// </summary>
        /// <param name="predicate">Выражение для сравнения</param>
        /// <returns>Отсутствие/присутствие записей</returns>
        public virtual bool ConfigExists(Func<Func<T, string[],string>, bool> predicate)
        {
            return command_map.Values.FirstOrDefault(predicate) != null;
        }

        /// <summary>
        /// Потокобезопасный Enumerable ToArray
        /// </summary>
        /// <returns></returns>
        public virtual Func<T, string[],string>[] GetConfigArray()
        {
            return command_map.Values.ToArray();
        }
    }
}
