using NSL.HttpClient.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace NSL.HttpClient
{
    public static class RequestOptionsExtensions
    {
        public static async Task RequestBuildProcess(this System.Net.Http.HttpClient client
            , BaseHttpRequestOptions options
            , HttpRequestMessage message
            , Func<Task<System.Net.Http.HttpContent>> defaultBuildAction
            , params object[] requestData)
        {
            var processor = options?.IOProcessor ?? DefaultHttpIOProcessor.Instance;

            if (processor == null) throw new InvalidOperationException("IOProcessor not set in options and no default(BaseHttpRequestOptions?.IOProcessor == null && DefaultHttpIOProcessor.Instance == null)");

            await processor.RequestBuildProcess(client, options, message, defaultBuildAction, requestData);
        }

        public static async Task ResponsePostProcess(this BaseHttpRequestOptions options
            , HttpResponseMessage httpResponse
            , BaseResponse response)
        {
            var processor = options?.IOProcessor ?? DefaultHttpIOProcessor.Instance;

            if (processor == null) throw new InvalidOperationException("IOProcessor not set in options and no default(BaseHttpRequestOptions?.IOProcessor == null && DefaultHttpIOProcessor.Instance == null)");

            await processor.ResponsePostProcess(options, httpResponse, response);
        }

        public static System.Net.Http.HttpClient FillClientOptions(this System.Net.Http.HttpClient client
            , BaseHttpRequestOptions options)
        {
            if (options?.ClientBuilder != null)
                return options.ClientBuilder(client);

            return client;
        }
    }
}
