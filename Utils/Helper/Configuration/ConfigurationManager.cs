using Newtonsoft.Json.Linq;
using SocketServer.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Utils.Helper.Configuration.Info;
using Utils.Logger;

namespace Utils.Helper.Configuration
{
    public class ConfigurationManager : ConfigurationStorage
    {
        private List<ConfigurationInfo> DefaultConfigurationList { get; set; }

        public ConfigurationManager(List<ConfigurationInfo> defaultConfigurationList)
        {
            SetDefaults(defaultConfigurationList);
            ReLoadData();

            LoggerStorage.Instance.main.AppendInfo( $"ConfigurationManager Loaded");
        }

        protected ConfigurationManager()
        {

        }

        /// <summary>
        /// Загрузка значений по умолчанию
        /// </summary>
        public ConfigurationManager SetDefaults(List<ConfigurationInfo> defaultConfigurationList)
        {
            DefaultConfigurationList = defaultConfigurationList;
            return this;
        }

        /// <summary>
        /// Загрузка данных с файла конфигурации
        /// </summary>
        public ConfigurationManager ReLoadData()
        {
            try
            {
                ClearStorage();
                DefaultConfigurationList?.ForEach(x => AddValue(x));

                var json_text = File.ReadAllText(System.IO.Directory.GetCurrentDirectory() + "/configuration.json");
                var json_data = JObject.Parse(json_text);
                var result = ParseArray("", json_data.Values());

                foreach (var item in result)
                {
                    AddValue(new Info.ConfigurationInfo(item.Key.ToLower(), item.Value, ""));
                }
            }
            catch (Exception ex)
            {
                LoggerStorage.Instance.main.AppendError($"ConfigurationManager Exception: {ex.ToString()}");
            }
            return this;
        }

        /// <summary>
        /// Добавить значение в конфигурацию
        /// </summary>
        /// <param name="name">Путь</param>
        /// <param name="value">Значение</param>
        /// <param name="client_value">Необходимо отправить клиенту</param>
        public void AddValue(string name, object value, string flags = "")
        {
            base.AddValue(new Info.ConfigurationInfo(name.ToLower(), value.ToString(), flags));
        }

        /// <summary>
        /// Получить значение
        /// </summary>
        /// <param name="name">Путь</param>
        /// <param name="only_client_value">Только клиенские значения</param>
        /// <returns></returns>
        public string GetValue(string path, string existFlag = "")
        {
            var v = base.GetValue(path.ToLower());

            if (v == null)
                return "";

            if (!string.IsNullOrEmpty(existFlag) && !v.ExistFlag(existFlag))
                return null;
            return v.Value;
        }

        /// <summary>
        /// Получить значение
        /// </summary>
        /// <param name="name">Путь</param>
        /// <param name="only_client_value">Только клиенские значения</param>
        /// <returns></returns>
        public T GetValue<T>(string path, string existFlag = "")
        {
            return (T)Convert.ChangeType(GetValue(path, existFlag), typeof(T));
        }

        public List<ConfigurationInfo> GetAllValues(string existFlag = "")
        {
            if (string.IsNullOrEmpty(existFlag))
                return config_map.Values.ToList();
            return config_map.Values.Where(x => x.ExistFlag(existFlag)).ToList();
        }

        /// <summary>
        /// Функция парсинга json данных
        /// </summary>
        /// <param name="path">Текущий путь</param>
        /// <param name="_prop">ветка с данными</param>
        /// <returns></returns>
        private List<KeyValuePair<string, string>> ParseArray(string path, IEnumerable<JToken> _prop)
        {
            List<KeyValuePair<string, string>> res = new List<KeyValuePair<string, string>>();
            foreach (var item in _prop.Where(x => x.Type != JTokenType.Array && x.Type != JTokenType.Object))
            {
                res.Add(new KeyValuePair<string, string>(path + (path != "" ? "/" : "") + ((JProperty)item.Parent).Name, item.Value<string>()));
            }
            foreach (var item in _prop.Where(x => x.Type == JTokenType.Array || x.Type == JTokenType.Object))
            {
                res.AddRange(ParseArray(path + (path != "" ? "/" : "") + ((JProperty)item.Parent).Name, item.Values()));
            }
            return res;
        }
    }
}
