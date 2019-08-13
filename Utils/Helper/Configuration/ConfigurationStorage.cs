﻿using System;
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

        public ConfigurationStorage()
        {
            config_map = new ConcurrentDictionary<string, ConfigurationInfo>();
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
                c.Flags = config.Flags;
                return;
            }

            config_map.TryAdd(config.Name, config);
        }

        /// <summary>
        /// Удалить значение
        /// </summary>
        /// <param name="name">Путь</param>
        public void RemoveValue(string name)
        {
            config_map.Remove(name, out ConfigurationInfo c);
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
        }
    }
}
