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
            if (options?.IOProcessor != null)
                await options.IOProcessor.RequestBuildProcess(client, options, message, defaultBuildAction, requestData);
            else
                message.Content = await defaultBuildAction();
        }

        public static async Task ResponsePostProcess(this BaseHttpRequestOptions options
            , HttpResponseMessage httpResponse
            , BaseResponse response)
        {
            if (options?.IOProcessor != null)
              await options.IOProcessor.ResponsePostProcess(options, httpResponse, response);
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
