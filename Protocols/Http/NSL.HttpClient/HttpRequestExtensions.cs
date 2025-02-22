using NSL.HttpClient.HttpContent;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.HttpClient
{
    public static class HttpRequestExtensions
    {
        public static Task<HttpResponseMessage> PostEmptyAsync(this System.Net.Http.HttpClient httpClient, string url,
            BaseHttpRequestOptions options = null)
            => httpClient.PostAsync(url, EmptyHttpContent.Instance, options?.CancellationToken ?? CancellationToken.None);

        public static Task<HttpResponseMessage> PostJsonAsync<TData>(this System.Net.Http.HttpClient httpClient, string url, TData data,
            BaseHttpRequestOptions options = null)
            => httpClient.PostAsync(url, JsonHttpContent.Create(data), options?.CancellationToken ?? CancellationToken.None);

        public static Task<HttpResponseMessage> PostBuildAsync(this System.Net.Http.HttpClient httpClient, string url, Func<System.Net.Http.HttpContent> builder,
            BaseHttpRequestOptions options = null)
            => httpClient.PostAsync(url, builder(), options?.CancellationToken ?? CancellationToken.None);

        //private static void abc()
        //{
        //    var h = new System.Net.Http.HttpClient();

        //    h.PostBuildAsync("", () => {
        //        var form = new FormHttpContent();

        //        form.Headers;

        //        form.
        //    })

        //}


    }
}
