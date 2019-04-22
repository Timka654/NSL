using Newtonsoft.Json.Linq;
using SocketServer.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Utils.Helper.Configuration.Info;

namespace Utils.Helper.Configuration
{
    public class ConfigurationManager : ConfigurationStorage
    {
        private List<ConfigurationInfo> DefaultConfigurationList { get; set; }

        public ConfigurationManager(List<ConfigurationInfo> defaultConfigurationList)
        {
            SetDefaults(defaultConfigurationList);
            ReLoadData();

            Utils.Logger.ConsoleLogger.WriteFormat(Utils.Logger.LoggerLevel.Info, $"ConfigurationManager Loaded");
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
                    AddValue(new Info.ConfigurationInfo() { Name = item.Key.ToLower(), Value = item.Value, ClientValue = false });
                }
            }
            catch (Exception ex)
            {
                Utils.Logger.ConsoleLogger.WriteFormat(Utils.Logger.LoggerLevel.Error, $"ConfigurationManager Exception: {ex.ToString()}");
            }
            return this;
        }

        /// <summary>
        /// Добавить значение в конфигурацию
        /// </summary>
        /// <param name="name">Путь</param>
        /// <param name="value">Значение</param>
        /// <param name="client_value">Необходимо отправить клиенту</param>
        public void AddValue(string name, object value, bool client_value)
        {
            base.AddValue(new Info.ConfigurationInfo() { Name = name.ToLower(), Value = value.ToString(), ClientValue = client_value });
        }

        /// <summary>
        /// Получить значение
        /// </summary>
        /// <param name="name">Путь</param>
        /// <param name="only_client_value">Только клиенские значения</param>
        /// <returns></returns>
        public string GetValue(string path, bool only_client_value = false)
        {
            var v = base.GetValue(path.ToLower());

            if (v == null)
                return "";

            if (only_client_value && v.ClientValue == false)
                return null;
            return v.Value;
        }

        /// <summary>
        /// Получить значение
        /// </summary>
        /// <param name="name">Путь</param>
        /// <param name="only_client_value">Только клиенские значения</param>
        /// <returns></returns>
        public T GetValue<T>(string path, bool only_client_value = false)
        {
            return (T)Convert.ChangeType(GetValue(path, only_client_value), typeof(T));
        }

        /// <summary>
        /// Генерация данных пакета с клиенскими конфигурациями
        /// </summary>
        /// <param name="packet">Исходящий пакетный буффер</param>
        public void WriteClientConfigPacketData(ref OutputPacketBuffer packet)
        {
            var arr = clientValues.Where(x => x.ClientValue).ToArray();
            packet.WriteInt32(arr.Length);
            foreach (var item in arr)
            {
                Info.ConfigurationInfo.WriteConfigurationPacketData(ref packet, item);
            }
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
