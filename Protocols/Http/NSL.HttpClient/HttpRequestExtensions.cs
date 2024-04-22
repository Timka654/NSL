using DevExtensions.Blazor.Http.HttpContent;
using System.Net.Http;
using System.Threading.Tasks;

namespace DevExtensions.Blazor.Http
{
    public static class HttpRequestExtensions
    {
        public static Task<HttpResponseMessage> PostEmptyAsync(this HttpClient httpClient, string url)
            => httpClient.PostAsync(url, EmptyHttpContent.Instance);

        public static Task<HttpResponseMessage> PostJsonAsync<TData>(this HttpClient httpClient, string url, TData data)
            => httpClient.PostAsync(url, JsonHttpContent.Create(data));
    }
}
