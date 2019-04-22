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
    public class ConsoleStorage
    {
        /// <summary>
        /// Список данных конфигураций
        /// </summary>
        protected ConcurrentDictionary<string, Func<INetworkClient, string[],string>> command_map;

        public ConsoleStorage()
        {
            command_map = new ConcurrentDictionary<string, Func<INetworkClient, string[],string>>();
        }

        /// <summary>
        /// Добавить значение конфигурации
        /// </summary>
        /// <param name="command">Данные конфигурации</param>
        public virtual void AddValue(string command, Func<INetworkClient, string[],string> commandArgs)
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
        public virtual Func<INetworkClient, string[],string> GetValue(string name)
        {
            command_map.TryGetValue(name.ToLower(), out Func<INetworkClient, string[],string> c);
            return c;
        }

        /// <summary>
        /// Потокобезопасный Linq Where
        /// </summary>
        /// <param name="predicate">Выражение для сравнения</param>
        /// <returns>Данные прошедшие фильтрацию</returns>
        public virtual IEnumerable<Func<INetworkClient, string[],string>> ConfigWhere(Func<Func<INetworkClient, string[],string>, bool> predicate)
        {
            return command_map.Values.Where(predicate);
        }

        /// <summary>
        /// Потокобезопасный Linq Exists
        /// </summary>
        /// <param name="predicate">Выражение для сравнения</param>
        /// <returns>Отсутствие/присутствие записей</returns>
        public virtual bool ConfigExists(Func<Func<INetworkClient, string[],string>, bool> predicate)
        {
            return command_map.Values.FirstOrDefault(predicate) != null;
        }

        /// <summary>
        /// Потокобезопасный Enumerable ToArray
        /// </summary>
        /// <returns></returns>
        public virtual Func<INetworkClient, string[],string>[] GetConfigArray()
        {
            return command_map.Values.ToArray();
        }
    }
}
