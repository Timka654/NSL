using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NSL.RestExtensions.RESTContentProcessors
{
    public class JsonSerializeRestContentProcessor : IRestContentProcessor
    {
        public virtual async Task<TValue> GetContent<TValue>(HttpResponseMessage message, HttpContent value)
        {
            var body = await message.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<TValue>(body);
        }

        public async Task<Dictionary<string, List<string>>> GetErrors(HttpResponseMessage message, HttpContent value)
        {
            var body = await message.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(body);
        }

        public virtual void SetContent<TValue>(HttpRequestMessage message, TValue value, Encoding textEncoding = null)
        {
            message.Content = new StringContent(JsonConvert.SerializeObject(value), textEncoding ?? Encoding.UTF8, "application/json");
        }
    }
}
