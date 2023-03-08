using NSL.RestExtensions.RESTContentProcessors;
using NSL.Utils.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

namespace NSL.RestExtensions.Unity
{
    public class UnityWebRequestRepository : UnityWebRequestRepository<DefaultRestContentProcessor>
    {
    }

    public class UnityWebRequestRepository<TConverter> : UnityWebRequestRepository<UnityHttpClientPool, UnityHttpClient, TConverter>
        where TConverter : IRestContentProcessor, new()
    {
        public UnityWebRequestRepository() : base()
        {
            clientPool = new UnityHttpClientPool(() => GetBaseDomain());
        }
    }

    public class UnityWebRequestRepository<TClientPool, TClient, TConverter> : NSL.RestExtensions.WebRequestRepository<TClientPool, TClient, TConverter>
        where TConverter : IRestContentProcessor, new()
        where TClient : HttpClient
        where TClientPool : HttpClientPool<TClient>
    {
        #region Utils

        public void ClearAuth()
        {
            SetDefaultHeader("Authorization", null);
            Debug.Log("Logout. Setted empty auth header.");
        }

        protected async void SafeRequest(string url, WebResponseDelegate onResult, Action<HttpRequestMessage> request)
        {
            SafeInvoke(await base.SafeRequest(url, request), onResult);
        }

        protected async void SafeRequest<TData>(string url, WebResponseDelegate<TData> onResult, Action<HttpRequestMessage> request = null)
        {
            SafeInvoke(await base.SafeRequest<TData>(url, request), onResult);
        }

        protected override Task LogRequestResult(HttpResponseMessage result, string responseContent)
        {
            if (!GetLogging())
                return Task.CompletedTask;

            var requestUriLine = $"RequestUri = {result.RequestMessage.RequestUri}\r\n";
            var requestContentLine = responseContent ?? string.Empty;
            var resultStatusCodeLine = $"ResultCode = {((int)result.StatusCode)}{(Enum.IsDefined(typeof(HttpStatusCode), result.StatusCode) ? $"({result.StatusCode})" : "")}\r\n";

            var stack = Environment.StackTrace;

            ThreadHelper.InvokeOnMain(() =>
            {
                if (responseContent != null)
                {
                    if (responseContent.Length < 10_000)
                    {
                        Debug.Log(requestUriLine + requestContentLine + resultStatusCodeLine +
                                  $"ResponseContent = {responseContent}\r\n" +
                                  $"================STACK TRACE================\r\n" +
                                  $"{stack}");
                    }
                    else if (responseContent.Length > 20_000)
                    {
                        Debug.Log(requestUriLine + requestContentLine + resultStatusCodeLine +
                                  $"ResponseContent = too large(20k more)\r\n" +
                                  $"================STACK TRACE================\r\n" +
                                  $"{stack}");
                    }
                    else
                    {

                        Debug.Log(requestUriLine + requestContentLine + resultStatusCodeLine +
                                  $"ResponseContent = ");

                        IEnumerable<char> charCollection = responseContent;

                        do
                        {
                            Debug.Log(new string(charCollection.Take(10_000).ToArray()));

                            charCollection = charCollection.Skip(10_000);
                        } while (charCollection.Any());

                        Debug.Log($"================STACK TRACE================\r\n" +
                                  $"{stack}");
                    }
                }
                else
                    Debug.Log(requestUriLine + requestContentLine + resultStatusCodeLine +
                              $"================STACK TRACE================\r\n" +
                              $"{stack}");
            });

            return Task.CompletedTask;
        }

        protected static void SafeInvoke(HttpRequestResult result, WebResponseDelegate onResult)
        {
            ThreadHelper.InvokeOnMain(() => onResult(result));
        }

        protected static async Task SafeInvoke(Task<HttpRequestResult> request, WebResponseDelegate onResult)
        {
            var result = await request;

            ThreadHelper.InvokeOnMain(() => onResult(result));
        }

        protected static void SafeInvoke<TData>(HttpRequestResult<TData> result, WebResponseDelegate<TData> onResult)
        {
            ThreadHelper.InvokeOnMain(() => onResult(result));
        }

        protected static async Task SafeInvoke<TData>(Task<HttpRequestResult<TData>> request, WebResponseDelegate<TData> onResult)
        {
            var result = await request;

            ThreadHelper.InvokeOnMain(() => onResult(result));
        }

        protected static async Task SafeInvoke<TRequest, TData>(TRequest requestData, Task<HttpRequestResult<TData>> request, WebResponseDelegate<TRequest, TData> onResult)
        {
            var result = await request;

            ThreadHelper.InvokeOnMain(() => onResult(requestData, result));
        }

        protected static async Task SafeInvoke<TRequest>(TRequest requestData, Task<HttpRequestResult> request, Action<TRequest, HttpRequestResult> onResult)
        {
            var result = await request;

            ThreadHelper.InvokeOnMain(() => onResult(requestData, result));
        }

        #endregion
    }
}
