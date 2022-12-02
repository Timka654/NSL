using Newtonsoft.Json.Linq;
using NSL.ConfigurationEngine.Info;
using NSL.SocketCore.Utils.Logger.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NSL.ConfigurationEngine.Providers.Json
{
    public class JsonConfigurationProvider : FileConfigurationProvider
    {
        private static List<JTokenType> supportedTypes = new List<JTokenType>
        {
            JTokenType.String,
            JTokenType.Date,
            JTokenType.Boolean,
            JTokenType.Integer,
            JTokenType.Float,
            JTokenType.Guid,
            JTokenType.TimeSpan
        };

        public JsonConfigurationProvider(string fileName, bool required = false, bool reloadOnChange = false) : base(fileName, required, reloadOnChange)
        {
        }

        public override bool LoadData()
        {
            if (!File.Exists(FileName))
            {
                if (!Required)
                    return true;
            }

            try
            {
                var json_text = File.ReadAllText(FileName);
                var json_data = JObject.Parse(json_text);

                ProcessJsonObject(json_data);
            }
            catch (Exception ex)
            {
                Manager.Log(LoggerLevel.Error, $"ConfigurationManager Exception: {ex.ToString()}");
                return false;
            }
            return true;
        }

        private void ProcessJsonObject(JObject obj, string parentPath = null)
        {
            var result = obj.PropertyValues()
                //.Where(x => supportedTypes.Contains(x.Type))
                .ToArray();

            foreach (var item in result)
            {
                ReadJsonProperty(item, parentPath);
                //manager.AddValue(new ConfigurationInfo(CorrectPath(item, manager), item.Value<string>(), ""));
            }
        }

        private void ReadJsonProperty(JToken property, string parentPath = null)
        {
            switch (property.Type)
            {
                case JTokenType.Date:
                case JTokenType.Guid:
                case JTokenType.Uri:
                case JTokenType.TimeSpan:
                case JTokenType.Integer:
                case JTokenType.Float:
                case JTokenType.String:
                case JTokenType.Boolean:
                    Update(new ConfigurationInfo(CorrectPath(property.Parent as JProperty, parentPath), property.Value<string>(), this, ""), true);
                    break;
                case JTokenType.Raw:
                    break;
                case JTokenType.None:
                    break;
                case JTokenType.Object:
                    ProcessJsonObject(property as JObject, CorrectPath(property.Parent as JProperty, parentPath));
                    break;
                case JTokenType.Array:
                    ReadJsonArray(property, parentPath);
                    break;
                case JTokenType.Constructor:
                    break;
                case JTokenType.Property:
                    break;
                case JTokenType.Comment:
                    break;
                case JTokenType.Null:
                    break;
                case JTokenType.Undefined:
                    break;
                case JTokenType.Bytes:
                    break;
                default:
                    break;
            }
        }

        private void ReadJsonArray(JToken property, string parentPath)
        {
            var name = CorrectPath(property.Parent as JProperty, parentPath);

            var arr = property as JArray;

            Update(new ConfigurationInfo($"{name}.count", arr.Count.ToString(), ""), true);

            for (int i = 0; i < arr.Count; i++)
            {
                var item = arr[i];

                if (!supportedTypes.Contains(item.Type))
                    continue;

                ReadJsonProperty(item, $"{name}.{i}");
            }
        }

        private string CorrectPath(JProperty jt, string parentPath)
        {
            var npath = jt?.Name;

            if (npath == null)
                npath = parentPath;
            else if (parentPath != null)
                npath = string.Join(".", parentPath, npath);

            npath = npath.Replace("['", ".");
            npath = npath.Replace("']", "");

            return npath.ToLower();
        }

        public override bool LoadData(byte[] data)
        {
            try
            {
                var json_text = Encoding.UTF8.GetString(data);
                var json_data = JObject.Parse(json_text);

                ProcessJsonObject(json_data);
            }
            catch (Exception ex)
            {
                Manager.Log(LoggerLevel.Error, $"ConfigurationManager Exception: {ex.ToString()}");
                return false;
            }
            return true;
        }

        public override bool SaveData()
        {
            var data = Manager.GetAllValues().OrderBy(x => x.Path);

            try
            {
                JObject jobj = new JObject();

                BuildNode(jobj, data);

                var json = jobj.ToString();

                File.WriteAllText($"{FileName}.d", json);

                return true;
            }
            catch (Exception ex)
            {
                Manager.Log(LoggerLevel.Error, $"ConfigurationManager Exception: {ex.ToString()}");
                return false;
            }
        }

        private void BuildNode(JObject jobj, IEnumerable<ConfigurationInfo> data, string path = null, int step = 0)
        {
            string[] nparsed = default;

            string npath = default;
            string fpath = default;

            List<string> processed = new List<string>();

            foreach (var item in data)
            {
                try
                {
                    nparsed = item.Path.Split('.');

                    npath = nparsed[step];

                    fpath = string.Join('.', path, npath).TrimStart('.');

                    var ndata = GroupPropertyes(data, fpath).ToArray();

                    if (ndata.Length == 0)
                    {
                        var obj = new JValue(item.Value);

                        jobj.Add(npath, obj);
                    }
                    else if (!processed.Contains(fpath))
                    {
                        processed.Add(fpath);
                        var obj = new JObject();

                        BuildNode(obj, ndata, fpath, step + 1);

                        jobj.Add(npath, obj);
                    }
                }
                catch (Exception ex)
                {

                    throw;
                }
            }
        }

        private IEnumerable<ConfigurationInfo> GroupPropertyes(IEnumerable<ConfigurationInfo> data, string startWith)
        {
            startWith = $"{startWith}.";

            return data.Where(x => x.Path.StartsWith(startWith));
        }
    }
}
