using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NSL.RestExtensions.ResponseReaders
{
    public delegate Task<object> HttpTypeContentConvertDelegate(HttpResponseMessage message, HttpContent value);

    public class TypeMapRestResponseReader : IRestResponseReader
    {
        Dictionary<Type, HttpTypeContentConvertDelegate> typeConvertMap = new Dictionary<Type, HttpTypeContentConvertDelegate>();

        public void AddType<TType>(HttpTypeContentConvertDelegate action)
        {
            if (typeConvertMap.ContainsKey(typeof(TType)))
            {
                typeConvertMap[typeof(TType)] = action;
                return;
            }

            typeConvertMap.Add(typeof(TType), action);
        }

        public async Task<TValue> GetContent<TValue>(HttpResponseMessage message, HttpContent value)
        {
            if (typeConvertMap.TryGetValue(typeof(TValue), out var action))
                return (TValue)await action(message, value);

            throw new KeyNotFoundException();

        }

        public async Task<Dictionary<string, List<string>>> GetErrors(HttpResponseMessage message, HttpContent value)
        {
            var content = await value.ReadAsStringAsync();

            var root = JToken.Parse(content);

            return root.ToDictionary(c => c.Path, c => c.ToList().Select(x => x.Value<string>()).ToList());
        }
    }
}
