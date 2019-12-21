using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ConfigurationEngine.Providers.Json
{
    public class LoadingProvider : IConfigurationLoadingProvider
    {
        public bool LoadData(IConfigurationManager manager)
        {
            try
            {
                var json_text = File.ReadAllText(Path.Combine(System.IO.Directory.GetCurrentDirectory(), manager.FileName));
                var json_data = JObject.Parse(json_text);
                var result = ParseArray(manager, "", json_data.Values());

                foreach (var item in result)
                {
                    manager.AddValue(new Info.ConfigurationInfo(item.Key.ToLower(), item.Value, ""));
                }
            }
            catch (Exception ex)
            {
                manager.Log(Logger.LoggerLevel.Error, $"ConfigurationManager Exception: {ex.ToString()}");
                return false;
            }
            return true;
        }

        private List<KeyValuePair<string, string>> ParseArray(IConfigurationManager manager, string path, IEnumerable<JToken> _prop)
        {
            List<KeyValuePair<string, string>> res = new List<KeyValuePair<string, string>>();
            foreach (var item in _prop.Where(x => x.Type != JTokenType.Array && x.Type != JTokenType.Object))
            {
                res.Add(new KeyValuePair<string, string>(path + (path != "" ? manager.NodeSeparator.ToString() : "") + ((JProperty)item.Parent).Name, item.Value<string>()));
            }
            foreach (var item in _prop.Where(x => x.Type == JTokenType.Array || x.Type == JTokenType.Object))
            {
                res.AddRange(ParseArray(manager, path + (path != "" ? manager.NodeSeparator.ToString() : "") + ((JProperty)item.Parent).Name, item.Values()));
            }
            return res;
        }
    }
}
