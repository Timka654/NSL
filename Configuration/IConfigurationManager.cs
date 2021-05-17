using ConfigurationEngine.Info;
using SCLogger;
using SocketCore.Utils.Logger.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ConfigurationEngine
{
    public abstract class IConfigurationManager : ConfigurationStorage
    {
        protected List<ConfigurationInfo> DefaultConfigurationList = new List<ConfigurationInfo>();

        public string FileName { get; protected set; }

        public event Action<LoggerLevel, string> OnLog = (ll, v) => { };

        public event Action OnLoad = () => { };

        public IConfigurationLoadingProvider Provider { get; protected set; }

        public IConfigurationManager(string fileName)
        {
            FileName = fileName;
            OnLog(LoggerLevel.Info, $"ConfigurationManager Loaded");
        }

        public bool ExistsFile()
        {
            return File.Exists(FileName);
        }

        /// <summary>
        /// Добавляет или измененияет значение и флаг найденой записи
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="flags"></param>
        public void AddValue(string name, object value, string flags = "")
        {
            base.AddValue(new ConfigurationInfo(name.ToLower(), value.ToString(), flags));
        }

        /// <summary>
        /// Добавляет или измененияет значение найденой записи
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AddValue(string name, object value)
        {
            base.AddValue(new ConfigurationInfo(name.ToLower(), value.ToString(), ""));
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
            return Provider.SaveData(this);
        }
    }

    public abstract class IConfigurationManager<T> : IConfigurationManager
        where T : IConfigurationManager<T>
    {
        public IConfigurationManager(string fileName) : base(fileName)
        {

        }

        /// <summary>
        /// Установка значений по умолчанию для перезагрузки
        /// </summary>
        public virtual T SetDefaults(List<ConfigurationInfo> defaultConfigurationList, bool reloading = false)
        {
            foreach (var item in defaultConfigurationList)
            {
                item.Path = item.Path.ToLower();
            }

            DefaultConfigurationList = defaultConfigurationList;

            if (reloading)
                ReloadData();

            return (T)this;
        }

        /// <summary>
        /// Пере/Загрузить значения с указанного источника с учетом данных по умолчанию
        /// </summary>
        /// <returns></returns>
        public virtual T ReloadData()
        {
            ClearStorage();
            DefaultConfigurationList?.ForEach(x => AddValue(x));
            Provider.LoadData(this);
            return (T)this;
        }
    }
}
