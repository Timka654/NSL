using NSL.RestExtensions.RESTContentProcessors;
using NSL.Utils.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
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

        /// <summary>
        /// Allow show large response/request content(2k+ chars) and split to small(10k chars) log messages, if need. In default LogRequestResult implementation
        /// </summary>
        protected virtual bool DisplayLargeLog { get; set; } = false;

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

        protected override async Task LogRequestResult(HttpResponseMessage result, string responseContent)
        {
            if (!GetLogging())
                return;

            bool showLarge = DisplayLargeLog;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"RequestUri = {result.RequestMessage.RequestUri}");

            string requestContentLine = string.Empty;

            if (result.RequestMessage.Content is StringContent rsc)
                requestContentLine = await rsc.ReadAsStringAsync();

            if (requestContentLine.Length > 2_000)
                sb.AppendLine($"RequestContent = too large(2k more),{nameof(DisplayLargeLog)} = {showLarge}");
            else
                sb.AppendLine($"RequestContent = {requestContentLine}");

            var resultStatusCodeLine = $"ResultCode = {(int)result.StatusCode}";

            if (Enum.IsDefined(typeof(HttpStatusCode), result.StatusCode))
                resultStatusCodeLine += $"({result.StatusCode})";

            sb.AppendLine(resultStatusCodeLine);

            var responseContentLine = responseContent ?? string.Empty;

            var stack = Environment.StackTrace;


            if (responseContentLine.Length > 2_000)
                sb.AppendLine($"ResponseContent = too large(2k more),{nameof(DisplayLargeLog)} = {showLarge}");
            else
                sb.AppendLine($"ResponseContent = {responseContentLine}");

            sb.AppendLine($"================STACK TRACE================");
            sb.AppendLine(stack);

            var message = sb.ToString();

            ThreadHelper.InvokeOnMain(() =>
            {
                Debug.Log(message);

                if(requestContentLine.Length > 2_000 && showLarge)
                {
                    int idx = 0;

                    IEnumerable<char> charCollection = requestContentLine;

                    do
                    {
                        var line = new string(charCollection.Take(10_000).ToArray());

                        Debug.Log($"request content pt.{idx} - {Environment.NewLine}{line}");

                        charCollection = charCollection.Skip(10_000);

                        ++idx;
                    } while (charCollection.Any());
                }

                if(responseContentLine.Length > 2_000 && showLarge)
                {
                    int idx = 0;

                    IEnumerable<char> charCollection = responseContentLine;

                    do
                    {

                        var line = new string(charCollection.Take(10_000).ToArray());

                        Debug.Log($"response content pt.{idx} - {Environment.NewLine}{line}");

                        charCollection = charCollection.Skip(10_000);

                        ++idx;
                    } while (charCollection.Any());
                }
            });
        }

        protected override void ErrorHandle(TClient client, HttpRequestMessage request, Exception ex)
        {
            Debug.LogError($"Web request error - {ex.ToString()}");
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
