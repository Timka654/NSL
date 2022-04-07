using Newtonsoft.Json.Linq;
using NSL.ConfigurationEngine;
using NSL.ConfigurationEngine.Info;
using SocketCore.Utils.Logger.Enums;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace ConfigurationEngine.Providers.Json
{
    public class LoadingProvider : IConfigurationLoadingProvider
    {
        public bool LoadData(IConfigurationManager manager)
        {
            try
            {
                var json_text = File.ReadAllText(manager.FileName);
                var json_data = JObject.Parse(json_text);

                var result = json_data.Descendants().Where(x=>x.Type == JTokenType.String || x.Type == JTokenType.Date || x.Type == JTokenType.Boolean || x.Type == JTokenType.Integer || x.Type == JTokenType.Float || x.Type == JTokenType.Guid || x.Type == JTokenType.TimeSpan);

                foreach (var item in result)
                {
                    manager.AddValue(new ConfigurationInfo(CorrectPath(item,manager), item.Value<string>(), ""));
                }
            }
            catch (Exception ex)
            {
                manager.Log(LoggerLevel.Error, $"ConfigurationManager Exception: {ex.ToString()}");
                return false;
            }
            return true;
        }

        private string CorrectPath(JToken jt, IConfigurationManager manager)
        {
            var npath = jt.Path;

            npath = npath.Replace("['", ".");
            npath = npath.Replace("']", "");

            return npath.ToLower();
        }

        public bool LoadData(IConfigurationManager manager, byte[] data)
        {
            try
            {
                var json_text = Encoding.UTF8.GetString(data);
                var json_data = JObject.Parse(json_text);

                //var result = ParseArray(manager, "", json_data.Values());

                //foreach (var item in result)
                //{
                //    manager.AddValue(new Info.ConfigurationInfo(item.Key.ToLower(), item.Value, ""));
                //}
            }
            catch (Exception ex)
            {
                manager.Log(LoggerLevel.Error, $"ConfigurationManager Exception: {ex.ToString()}");
                return false;
            }
            return true;
        }

        public bool SaveData(IConfigurationManager manager)
        {
            var data = manager.GetAllValues();
            try
            {
                JObject jobj = new JObject();

                using (var writer = jobj.CreateWriter())
                {

                    foreach (var item in data)
                    {
                        writer.WritePropertyName(item.Path);

                                writer.WriteValue(item.Value);
                    }
                }
                var json = jobj.ToString();

                File.WriteAllText(manager.FileName, json);

                return true;
            }
            catch (Exception ex)
            {
                manager.Log(LoggerLevel.Error, $"ConfigurationManager Exception: {ex.ToString()}");
                return false;
            }
        }

        private string GetPath(IConfigurationManager manager, JToken jt)
        {
            return jt.Path;
        }
    }
}
