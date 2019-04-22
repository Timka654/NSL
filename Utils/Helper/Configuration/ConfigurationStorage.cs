using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils.Helper.Configuration.Info;

namespace Utils.Helper.Configuration
{
    public class ConfigurationStorage
    {
        /// <summary>
        /// Список данных конфигураций
        /// </summary>
        protected ConcurrentDictionary<string, ConfigurationInfo> config_map;

        /// <summary>
        /// Список конфигураций для клиента
        /// </summary>
        protected List<ConfigurationInfo> clientValues;

        public ConfigurationStorage()
        {
            config_map = new ConcurrentDictionary<string, ConfigurationInfo>();
            clientValues = new List<ConfigurationInfo>();
        }

        /// <summary>
        /// Добавить значение
        /// </summary>
        /// <param name="config">Данные конфигурации</param>
        public void AddValue(ConfigurationInfo config)
        {
            if (config_map.TryGetValue(config.Name, out ConfigurationInfo c))
            {
                c.Value = config.Value;
                c.ClientValue = config.ClientValue;
                return;
            }

            config_map.TryAdd(config.Name, config);
            if (config.ClientValue)
                clientValues.Add(config);
        }

        /// <summary>
        /// Удалить значение
        /// </summary>
        /// <param name="name">Путь</param>
        public void RemoveValue(string name)
        {
            if (config_map.Remove(name, out ConfigurationInfo c))
            {
                if (c.ClientValue)
                {
                    clientValues.Remove(c);
                }
            }
        }

        /// <summary>
        /// Получить значение
        /// </summary>
        /// <param name="name">Путь</param>
        /// <returns></returns>
        public ConfigurationInfo GetValue(string name)
        {
            config_map.TryGetValue(name, out ConfigurationInfo c);
            return c;
        }

        /// <summary>
        /// Потокобезопасный Linq Where
        /// </summary>
        /// <param name="predicate">Выражение для сравнения</param>
        /// <returns>Данные прошедшие фильтрацию</returns>
        public IEnumerable<ConfigurationInfo> ConfigWhere(Func<ConfigurationInfo, bool> predicate)
        {
            var r = config_map.Values.Where(predicate);
            return r;

        }

        /// <summary>
        /// Потокобезопасный Linq Exists
        /// </summary>
        /// <param name="predicate">Выражение для сравнения</param>
        /// <returns>Отсутствие/присутствие записей</returns>
        public bool ConfigExists(Func<ConfigurationInfo, bool> predicate)
        {
            var r = config_map.Values.FirstOrDefault(predicate);
            return r != null;
        }

        /// <summary>
        /// Потокобезопасный Enumerable ToArray
        /// </summary>
        /// publicreturns></returns>
        public ConfigurationInfo[] GetConfigArray()
        {
            var r = config_map.Values.ToArray();
            return r;
        }

        public void ClearStorage()
        {
            config_map.Clear();
            clientValues.Clear();
        }
    }
}
