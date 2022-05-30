using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NSL.RestExtensions
{
    public static class HttpExtensions
    {
        public static async Task<TResult> GetResponseBody<TResult>(this HttpResponseMessage response)
        {
            if (typeof(TResult).Equals(typeof(MemoryStream)))
            {
                var outStream = new MemoryStream();
                await (await response.Content.ReadAsStreamAsync()).CopyToAsync(outStream);

                return (TResult)(object)outStream;
            }
            else if (typeof(TResult).Equals(typeof(StringReader)))
            {
                return (TResult)(object)(new StringReader(await response.Content.ReadAsStringAsync()));
            }

            var body = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<TResult>(body);
        }

        public static HttpRequestMessage SetRequestBody<TValue>(this HttpRequestMessage request, TValue data)
        {
            request.Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, BaseHttpRequestResult.DefaultJsonMimeType);

            return request;
        }
    }
}
