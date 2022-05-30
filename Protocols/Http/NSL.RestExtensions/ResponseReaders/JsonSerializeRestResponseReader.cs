using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NSL.RestExtensions.ResponseReaders
{
    public class JsonSerializeRestResponseReader : IRestResponseReader
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
    }
}
