using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;

namespace Utils
{
    public class WebHelper
    {
        NameValueCollection parameters = new NameValueCollection();

        string host;

        string path;

        public WebHelper(string host, string path)
        {
            this.host = host;
            this.path = path;

            if (this.host.EndsWith("/"))
                this.host = this.host.Substring(0, this.host.Length - 1);
            else
                this.host = host;

            if (this.path.StartsWith("/"))
                this.path = this.path.Substring(1, this.host.Length - 1);
            else
                this.path = path;
        }

        public void AddParameter(string name, string value)
        {
            parameters.Add(name, value);
        }

        public string GetString(string method)
        {
            byte[] result = null;
            using (WebClient wc = new WebClient())
            {
                result = wc.UploadValues(host, method, parameters);
            }

            if (result == null)
                return "";

            return Encoding.ASCII.GetString(result);
        }

        public object GetJson(string method)
        {
            string text = GetString(method);
            var json = JsonSerializer.Create();

            return json.Deserialize(new JsonTextReader(new StringReader(text)));
        }

        public T GetJson<T>(string method)
        {
            string text = GetString(method);
            var json = JsonSerializer.Create();

            return json.Deserialize<T>(new JsonTextReader(new StringReader(text)));
        }

        public T ToObject<T>(string text)
        {
            var json = JsonSerializer.Create();

            return json.Deserialize<T>(new JsonTextReader(new StringReader(text)));
        }
    }
}
