using NSL.HttpClient.HttpContent;
using System.Net.Http;
using System.Threading.Tasks;

namespace NSL.HttpClient
{
    public static class HttpRequestExtensions
    {
        public static Task<HttpResponseMessage> PostEmptyAsync(this System.Net.Http.HttpClient httpClient, string url)
            => httpClient.PostAsync(url, EmptyHttpContent.Instance);

        public static Task<HttpResponseMessage> PostJsonAsync<TData>(this System.Net.Http.HttpClient httpClient, string url, TData data)
            => httpClient.PostAsync(url, JsonHttpContent.Create(data));
    }
}
