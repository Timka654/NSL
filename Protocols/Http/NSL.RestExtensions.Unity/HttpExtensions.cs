using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RestExtensions.Unity
{
    public static class HttpExtensions
    {
        public static async Task<TResult> GetResponseBody<TResult>(this HttpResponseMessage response)
        {
            var body = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<TResult>(body);
        }

        public static HttpRequestMessage SetRequestBody<TValue>(this HttpRequestMessage request, TValue data)
        {
            request.Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "text/json");

            return request;
        }
    }
}
