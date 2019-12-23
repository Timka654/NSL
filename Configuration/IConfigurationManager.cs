﻿using ConfigurationEngine.Info;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConfigurationEngine
{
    public abstract class IConfigurationManager : ConfigurationStorage
    {
        protected List<ConfigurationInfo> DefaultConfigurationList = new List<ConfigurationInfo>();

        public string FileName { get; protected set; }

        public event Action<LoggerLevel, string> OnLog = (ll, v) => { };

        public IConfigurationLoadingProvider Provider { get; protected set; }

        public char NodeSeparator { get; protected set; }

        public IConfigurationManager(string fileName, char nodeSeparator = '/') : this(nodeSeparator)
        {
            FileName = fileName;
            OnLog(LoggerLevel.Info, $"ConfigurationManager Loaded");
        }

        protected IConfigurationManager(char nodeSeparator = '/')
        {
            NodeSeparator = nodeSeparator;
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
            return (T)Convert.ChangeType(GetValue(path, existFlag), typeof(T));
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
    }

    public abstract class IConfigurationManager<T> : IConfigurationManager
        where T : IConfigurationManager<T>
    {
        public IConfigurationManager(string fileName, char nodeSeparator = '/') : base(fileName, nodeSeparator)
        { 
        
        }

        /// <summary>
        /// Установка значений по умолчанию для перезагрузки
        /// </summary>
        public virtual T SetDefaults(List<ConfigurationInfo> defaultConfigurationList, bool reloading = false)
        {
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
