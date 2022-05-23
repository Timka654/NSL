using NSL.ConfigurationEngine.Info;
using System.Collections.Concurrent;
using System.Linq;

namespace NSL.ConfigurationEngine
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
        public ConfigurationInfo AddValue(ConfigurationInfo config)
        {
            if (config_map.TryGetValue(config.Path, out ConfigurationInfo c))
            {
                c.Value = config.Value;
                c.Flags = config.Flags;
                c.Provider = config.Provider;
                return c;
            }

            config_map.TryAdd(config.Path, config);

            return config;
        }

        /// <summary>
        /// Удалить значение
        /// </summary>
        /// <param name="name">Путь</param>
        public void RemoveValue(string name)
        {
            config_map.TryRemove(name, out ConfigurationInfo c);
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

        public bool ExistValue(string name)
        {
            return config_map.ContainsKey(name);
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

        public void CopyFrom(ConfigurationStorage otherStorage)
        {
            foreach (var item in otherStorage.config_map)
            {
                AddValue(item.Value);
            }
        }
    }
}
