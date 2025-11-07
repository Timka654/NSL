using NSL.HttpClient.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace NSL.HttpClient
{
    public interface IHttpIOProcessor
    {
        Task RequestBuildProcess(System.Net.Http.HttpClient client
            , BaseHttpRequestOptions options
            , HttpRequestMessage message
            , Func<Task<System.Net.Http.HttpContent>> defaultBuildAction
            , params object[] requestData);

        Task ResponsePostProcess(BaseHttpRequestOptions options
            , HttpResponseMessage httpResponse
            , BaseResponse response);
    }

    public class DefaultHttpIOProcessor : IHttpIOProcessor
    {
        public static IHttpIOProcessor Instance { get; private set; } = new DefaultHttpIOProcessor();

        public const string HttpClientKey = "__nsl__http_client";
        public const string HttpOptionsKey = "__nsl__http_options";

        public static void SetInstance(IHttpIOProcessor processor)
        {
            if (processor == null)
                throw new ArgumentNullException(nameof(processor));

            Instance = processor;
        }

        public virtual async Task RequestBuildProcess(System.Net.Http.HttpClient client
            , BaseHttpRequestOptions options
            , HttpRequestMessage message
            , Func<Task<System.Net.Http.HttpContent>> defaultBuildAction
            , params object[] requestData)
        {
            message.Content = await defaultBuildAction();

#if UNITY
            message.Properties.TryAdd(HttpClientKey, client);
            message.Properties.TryAdd(HttpOptionsKey, options);
#else
            message.Options.TryAdd(HttpClientKey, client);
            message.Options.TryAdd(HttpOptionsKey, options);
#endif
            if (options == null) return;

#if UNITY
            foreach (var item in options.ObjectBag)
            {
                message.Properties.TryAdd(item.Key, item.Value);
            }

#else
            foreach (var item in options.ObjectBag)
            {
                message.Options.TryAdd(item.Key, item.Value);
            }
#endif
           
        }

        public virtual Task ResponsePostProcess(BaseHttpRequestOptions options
            , HttpResponseMessage httpResponse
            , BaseResponse response)
        {
            return Task.CompletedTask;
        }
    }
}
