using NSL.ConfigurationEngine.Info;
using NSL.ConfigurationEngine.Providers;
using NSL.SocketCore.Utils.Logger.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace NSL.ConfigurationEngine
{
    public abstract class BaseConfigurationManager : ConfigurationStorage
    {
        public static readonly NoProviderLoadingProvider NoProvider = new NoProviderLoadingProvider();

        protected List<ConfigurationInfo> DefaultConfigurationList = new List<ConfigurationInfo>();

        public event Action<LoggerLevel, string> OnLog = (ll, v) => { };

        public event Action OnLoad = () => { };

        public ReadOnlyCollection<IConfigurationProvider> Providers => new ReadOnlyCollection<IConfigurationProvider>(providers);

        private List<IConfigurationProvider> providers { get; } = new List<IConfigurationProvider>();

        public BaseConfigurationManager()
        {
            OnLog(LoggerLevel.Info, $"ConfigurationManager Loaded");
        }

        public BaseConfigurationManager AddProvider(IConfigurationProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            provider.Manager = this;

            providers.Add(provider);

            return this;
        }

        /// <summary>
        /// Добавляет или изменяет значение и флаг найденой записи
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="flags"></param>
        public void AddValue(string name, object value, string flags = "")
        {
            var v = base.GetValue(name);

            AddValue(name.ToLower(), value.ToString(), v == null ? NoProvider : v.Provider, flags);
        }

        public void AddValue(string name, object value, IConfigurationProvider provider, string flags = "")
        {
            Update(new ConfigurationInfo(name.ToLower(), value.ToString(), provider, flags), true);
        }

        private void Update(ConfigurationInfo configuration, bool forceAdd)
        {
            configuration.Provider.Update(configuration, forceAdd);
        }

        /// <summary>
        /// Добавляет или изменяет значение найденой записи
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AddValue(string name, object value)
        {
            AddValue(name.ToLower(), value.ToString(), "");
        }

        /// <summary>
        /// Получить значение по указаному пути
        /// </summary>
        /// <param name="path"></param>
        /// <param name="existFlag">Оставить стандартным для получения значения без учета флага</param>
        /// <returns></returns>
        public string GetValue(string path, string existFlag = "")
        {
            var v = base.GetValue(path.ToLower());

            if (v == null)
                return default;

            if (!string.IsNullOrWhiteSpace(existFlag) && !v.ExistFlag(existFlag))
                return default;

            return v.Value;
        }

        /// <summary>
        /// Получить значение по указаному пути
        /// </summary>
        /// <param name="path"></param>
        /// <param name="existFlag">Оставить стандартным для получения значения без учета флага</param>
        /// <returns></returns>
        public T GetValue<T>(string path, string existFlag = "")
        {
            var val = GetValue(path, existFlag);

            if (val == null)
                return default;

            return (T)Convert.ChangeType(val, typeof(T));
        }

        /// <summary>
        /// Получить значение по указаному пути
        /// </summary>
        /// <param name="path"></param>
        /// <param name="existFlag">Оставить стандартным для получения значения без учета флага</param>
        /// <returns></returns>
        public TEnum GetEnumValue<TEnum>(string path, string existFlag = "", bool ignoreCase = false) 
            where TEnum : struct
        {
            var val = GetValue(path, existFlag);

            if (val == null)
                return default;

            return Enum.Parse<TEnum>(val, ignoreCase);
        }

        /// <summary>
        /// Получить все записи
        /// </summary>
        /// <param name="existFlag">Оставить стандартным для получения значения без учета флага</param>
        /// <returns></returns>
        public List<ConfigurationInfo> GetAllValues(string existFlag = "")
        {
            if (string.IsNullOrWhiteSpace(existFlag))
                return config_map.Values.ToList();
            return config_map.Values.Where(x => x.ExistFlag(existFlag)).ToList();
        }

        /// <summary>
        /// Вызов события OnLog
        /// </summary>
        /// <param name="level">Тип сообщения</param>
        /// <param name="content">текст</param>
        public void Log(LoggerLevel level, string content)
        {
            OnLog(level, content);
        }

        public virtual bool SaveData()
        {
            return providers.All(e => e.SaveData());
        }


        /// <summary>
        /// Установка значений по умолчанию для перезагрузки
        /// </summary>
        public bool SetDefaults(List<ConfigurationInfo> defaultConfigurationList, bool reloading = false)
        {
            foreach (var item in defaultConfigurationList)
            {
                item.Path = item.Path.ToLower();
            }

            DefaultConfigurationList = defaultConfigurationList;

            if (reloading)
                return ReloadData();

            return true;
        }

        /// <summary>
        /// Пере/Загрузить значения с указанного источника с учетом данных по умолчанию
        /// </summary>
        /// <returns></returns>
        public bool ReloadData()
        {
            ClearStorage();
            DefaultConfigurationList?.ForEach(x => AddValue(x));

            if (providers == null)
                throw new NullReferenceException($"{nameof(providers)} cannot be null");

            return providers.All(e => e.LoadData());
        }
    }

    public abstract class IConfigurationManager<T> : BaseConfigurationManager
        where T : IConfigurationManager<T>
    {
        public IConfigurationManager(string fileName)
        {

        }

        /// <summary>
        /// Установка значений по умолчанию для перезагрузки
        /// </summary>
        public new virtual T SetDefaults(List<ConfigurationInfo> defaultConfigurationList, bool reloading = false)
        {
            base.SetDefaults(defaultConfigurationList, reloading);

            return (T)this;
        }

        /// <summary>
        /// Пере/Загрузить значения с указанного источника с учетом данных по умолчанию
        /// </summary>
        /// <returns></returns>
        public new virtual T ReloadData()
        {
            base.ReloadData();

            return (T)this;
        }
    }
}
