using NSL.ConfigurationEngine.Info;
using System.Collections.Generic;
using System.IO;

namespace ConfigurationEngine.Providers.Json
{
    public class JsonReader
    {
        public static IEnumerable<ConfigurationInfo> ReadJson(string json)
        {
            using var sr = new System.IO.StringReader(json);
            return ReadJson(sr);
        }

        public static IEnumerable<ConfigurationInfo> ReadJson(TextReader jsonReader)
        {
            List<ConfigurationInfo> result = new List<ConfigurationInfo>();

            string currentPath = "";
            bool activePath = false;
            string currentValue = "";
            bool activeValue = false;
            char nextChar = ReadNext(jsonReader);

            if (nextChar != '{')
            {
                throw new System.Data.SyntaxErrorException("json must start by '{' character");
            }

            while (jsonReader.Peek() > -1)
            {
                nextChar = ReadNext(jsonReader);

                if (activeValue)
                {
                    if (activePath)
                    {
                        currentValue += nextChar;
                    }
                    else
                    {
                        if (nextChar == '{')
                        {
                            result.AddRange(ReadObject(jsonReader, currentPath));
                            activeValue = false;
                        }
                        if (nextChar == ',')
                        {
                            activeValue = false;
                            result.Add(new ConfigurationInfo(currentPath, currentValue, ""));
                        }
                    }
                }
                else
                {
                    if (nextChar == ':')
                    {
                        activeValue = true;
                        continue;
                    }

                    if (nextChar == '\"')
                    {
                        if (activePath)
                        {
                            activePath = false;
                            continue;
                        }
                        activePath = true;
                        currentPath = string.Empty;
                    }
                }
            }


            return result;
        }

        private static IEnumerable<ConfigurationInfo> ReadObject(TextReader jsonReader, string path)
        {
            List<ConfigurationInfo> result = new List<ConfigurationInfo>();


            return result;
        }

        private static char ReadNext(TextReader jsonReader, bool value = false)
        {
            char r = (char)0;
            while (jsonReader.Peek() > -1)
            {
                r = (char)jsonReader.Read();
                if ((value || (r != ' ' && r != '\t' && r != '\n' && r != '\r')))
                    break;
            }
            return r;
        }
    }
}
