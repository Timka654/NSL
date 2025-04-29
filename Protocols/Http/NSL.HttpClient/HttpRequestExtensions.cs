using NSL.HttpClient.HttpContent;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.HttpClient
{
    public static class HttpRequestExtensions
    {
        public static Task<HttpResponseMessage> PostEmptyAsync(this System.Net.Http.HttpClient httpClient
            , string url
            , BaseHttpRequestOptions options = null)
            => httpClient.PostAsync(url, EmptyHttpContent.Instance, options?.CancellationToken ?? CancellationToken.None);

        public static Task<HttpResponseMessage> PostJsonAsync<TData>(this System.Net.Http.HttpClient httpClient
            , string url
            , TData data
            , BaseHttpRequestOptions options = null)
            => httpClient.PostAsync(url, JsonHttpContent.Create(data), options?.CancellationToken ?? CancellationToken.None);

        public static async Task<HttpResponseMessage> PostBuildAsync(this System.Net.Http.HttpClient httpClient
            , string url, Func<System.Net.Http.HttpClient, HttpRequestMessage, Task> builder
            , BaseHttpRequestOptions options = null)
        {
            var httpMessage = new HttpRequestMessage(HttpMethod.Post, url);

            await builder(httpClient, httpMessage);

            return await httpClient.SendAsync(httpMessage, options?.CancellationToken ?? CancellationToken.None);
        }

        public static Task<HttpResponseMessage> PostBuildAsync(this System.Net.Http.HttpClient httpClient
            , string url
            , Action<System.Net.Http.HttpClient, HttpRequestMessage> builder
            , BaseHttpRequestOptions options = null)
        {
            var httpMessage = new HttpRequestMessage(HttpMethod.Post, url);

            builder(httpClient, httpMessage);

            return httpClient.SendAsync(httpMessage, options?.CancellationToken ?? CancellationToken.None);
        }
    }
}
