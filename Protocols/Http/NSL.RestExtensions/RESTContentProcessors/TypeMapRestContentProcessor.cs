using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NSL.RestExtensions.RESTContentProcessors
{
    public delegate Task<object> HttpTypeContentGetContentDelegate(HttpResponseMessage message, HttpContent value);
    public delegate void HttpTypeContentSetContentDelegate(HttpRequestMessage message, object value, Encoding textEncoding);

    internal class TypeMapDelegateContainer
    {
        public HttpTypeContentGetContentDelegate GetContentDelegate;
        public HttpTypeContentSetContentDelegate SetContentDelegate;
    }

    public class TypeMapRestContentProcessor : IRestContentProcessor
    {
        Dictionary<Type, TypeMapDelegateContainer> typeConvertMap = new Dictionary<Type, TypeMapDelegateContainer>();

        public void AddReadType<TType>(HttpTypeContentGetContentDelegate action)
        {
            if (typeConvertMap.TryGetValue(typeof(TType), out var ex))
            {
                ex.GetContentDelegate = action;
                return;
            }

            typeConvertMap.Add(typeof(TType), new TypeMapDelegateContainer() { GetContentDelegate = action });
        }

        public void AddWriteType<TType>(HttpTypeContentSetContentDelegate action)
        {
            if (typeConvertMap.TryGetValue(typeof(TType), out var ex))
            {
                ex.SetContentDelegate = action;
                return;
            }

            typeConvertMap.Add(typeof(TType), new TypeMapDelegateContainer() { SetContentDelegate = action });
        }

        public async Task<TValue> GetContent<TValue>(HttpResponseMessage message, HttpContent value)
        {
            if (typeConvertMap.TryGetValue(typeof(TValue), out var action))
            {
                if (action.GetContentDelegate == null)
                    throw new ArgumentNullException($"Read type action not found for type {typeof(TValue)}");

                return (TValue)await action.GetContentDelegate(message, value);
            }

            throw new KeyNotFoundException($"type {typeof(TValue)} not found");
        }

        public async Task<Dictionary<string, List<string>>> GetErrors(HttpResponseMessage message, HttpContent value)
        {
            var content = await value.ReadAsStringAsync();

            var root = JToken.Parse(content);

            return root.ToDictionary(c => c.Path, c => c.ToList().Select(x => x.Value<string>()).ToList());
        }

        public void SetContent<TValue>(HttpRequestMessage message, TValue value, Encoding textEncoding = null)
        {
            if (typeConvertMap.TryGetValue(typeof(TValue), out var action))
            {
                if (action.GetContentDelegate == null)
                    throw new ArgumentNullException($"Write type action not found for type {typeof(TValue)}");

                action.SetContentDelegate(message, value, textEncoding ?? Encoding.UTF8);
            }

            throw new KeyNotFoundException($"type {typeof(TValue)} not found");
        }
    }
}
