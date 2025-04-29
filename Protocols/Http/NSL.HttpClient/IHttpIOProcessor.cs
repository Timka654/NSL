using NSL.HttpClient.Models;
using System;
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
        public virtual async Task RequestBuildProcess(System.Net.Http.HttpClient client
            , BaseHttpRequestOptions options
            , HttpRequestMessage message
            , Func<Task<System.Net.Http.HttpContent>> defaultBuildAction
            , params object[] requestData)
        {
            message.Content = await defaultBuildAction();
        }

        public virtual Task ResponsePostProcess(BaseHttpRequestOptions options, HttpResponseMessage httpResponse, BaseResponse response)
        {
            return Task.CompletedTask;
        }
    }
}
