using RestExtensions.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using SCL.Unity;
using UnityEngine;

namespace RestExtensions.Unity
{
    public delegate void WebResponseDelegate(HttpRequestResult request);

    public delegate void WebResponseDelegate<TResult>(HttpRequestResult<TResult> request);

    //public delegate void WebResponseDelegate<TRequest>(TRequest requestData,HttpRequestResult request);

    public delegate void WebResponseDelegate<TRequest, TResult>(TRequest requestData, HttpRequestResult<TResult> request);

    public class BaseWebRequests
    {
        public virtual int GetTimeout() => 10000;

        public virtual string GetBaseDomain() => "http://127.0.0.1/";

        public virtual bool GetLogging() => true;

        public virtual HttpMethod GetHttpMethod() => HttpMethod.Post;


        #region Utils

        public void ClearAuth()
        {
            SetDefaultHeader("Authorization", null);
            Debug.Log("Logout. Setted empty auth header.");
        }

        public void SetDefaultHeader(string name, string value) => clientPool.SetDefaultHeader(name, value);

        public void FreeClient(HttpClient client, BaseHttpRequestResult response = null) => FreeClient(client, response?.MessageResponse);

        public void FreeClient(HttpClient client, HttpResponseMessage response = null) => clientPool.FreeClient(client, response);

        public void FreeClient(HttpClient client) => clientPool.FreeClient(client);

        protected async void SafeRequest(string url, WebResponseDelegate onResult, Action<HttpRequestMessage> request)
        {
            var client = await GetClient();

            var result = await SafeRequest(client, url, request);

            FreeClient(client);

            SafeInvoke(result, onResult);
        }

        protected async void SafeRequest<TData>(string url, WebResponseDelegate<TData> onResult, Action<HttpRequestMessage> request = null)
        {
            var client = await GetClient();

            var result = await SafeRequest<TData>(client, url, request);

            FreeClient(client);

            SafeInvoke(result, onResult);
        }

        protected async Task<HttpRequestResult> SafeRequest(string url, Action<HttpRequestMessage> request, bool dispose = false)
        {
            var client = await GetClient();

            var result = await SafeRequest(client, url, request);

            FreeClient(client, dispose ? result : null);

            return result;
        }

        protected async Task<HttpRequestResult<TData>> SafeRequest<TData>(string url, Action<HttpRequestMessage> request = null, bool dispose = false)
        {
            var client = await GetClient();

            var result = await SafeRequest<TData>(client, url, request);

            FreeClient(client, dispose ? result : null);

            return result;
        }
        protected async Task<HttpRequestResult> SafeRequest(HttpClient client, string url, Action<HttpRequestMessage> request)
        {
            var message = new HttpRequestMessage(HttpMethod.Post, url);

            request.Invoke(message);

            return await SafeRequest(client, message);
        }

        protected async Task<HttpRequestResult<TData>> SafeRequest<TData>(HttpClient client, string url, Action<HttpRequestMessage> request = null)
        {
            var message = new HttpRequestMessage(HttpMethod.Post, url);

            if (request != null)
                request.Invoke(message);

            return await SafeRequest<TData>(client, message);
        }
        protected async Task<HttpRequestResult> SafeRequest(HttpClient client, HttpRequestMessage request)
        {
            var result = await SafeSendRequest(client, request);

            await LogRequestResult(result);

            HttpRequestResult r = await ProcessBaseResponse<HttpRequestResult>(result) ?? new HttpRequestResult(result);

            FreeClient(client, result);

            return r;
        }

        protected async Task<HttpRequestResult<TData>> SafeRequest<TData>(HttpClient client, HttpRequestMessage request)
        {
            var result = await SafeSendRequest(client, request);

            await LogRequestResult(result);

            HttpRequestResult<TData> r = await ProcessBaseResponse<HttpRequestResult<TData>>(result) ?? new HttpRequestResult<TData>(result, await result.GetResponseBody<TData>());

            FreeClient(client, result);

            return r;
        }

        private async Task<TResult> ProcessBaseResponse<TResult>(HttpResponseMessage response)
            where TResult : BaseHttpRequestResult, new()
        {
            if (response.IsSuccessStatusCode)
                return default;
            else if (response.StatusCode == HttpStatusCode.BadRequest)
                return new TResult()
                {
                    MessageResponse = response,
                    ErrorMessages = await response.GetResponseBody<Dictionary<string, List<string>>>()
                };


            return new TResult() { MessageResponse = response };
        }

        protected static async Task<HttpResponseMessage> SafeSendRequest(HttpClient client, HttpRequestMessage request)
        {
            HttpResponseMessage result = default;

            try
            {
                result = await client.SendAsync(request);
            }
#pragma warning disable CS0168 // Переменная объявлена, но не используется
            catch (HttpRequestException ex)
#pragma warning restore CS0168 // Переменная объявлена, но не используется
            {
                result = new HttpResponseMessage(HttpStatusCode.InternalServerError) { RequestMessage = request };

            }
            catch (TaskCanceledException)
            {
                result = new HttpResponseMessage(HttpStatusCode.RequestTimeout) { RequestMessage = request };
            }

            return result;
        }

        protected HttpClientPool clientPool;

        protected async Task<HttpClient> GetClient()
        {
            return await clientPool.GetClient(requestTimeout: TimeSpan.FromMilliseconds(GetTimeout()));
        }

        protected virtual async Task LogRequestResult(HttpResponseMessage result)
        {
            string resultContent = result.Content != null ? await result.Content.ReadAsStringAsync() : null;
            await LogRequestResult(result, resultContent);

        }
        protected virtual async Task LogRequestResult(HttpResponseMessage result, string responseContent)
        {
            if (!GetLogging())
                return;

            var requestUriLine = $"RequestUri = { result.RequestMessage.RequestUri }\r\n";
            var requestContentLine = result.RequestMessage.Content is StringContent sc ? $"RequestContent = { await sc.ReadAsStringAsync() }\r\n" : string.Empty;
            var resultStatusCodeLine = $"ResultCode = { result.StatusCode }\r\n";

            if (responseContent != null)
            {
                if (responseContent.Length < 10_000)
                {
                    Debug.Log(requestUriLine + requestContentLine + resultStatusCodeLine +
                              $"ResponseContent = { responseContent }\r\n" +
                              $"================STACK TRACE================\r\n" +
                              $"{ Environment.StackTrace }");
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
                              $"{ Environment.StackTrace }");
                }
            }
            else
                Debug.Log(requestUriLine + requestContentLine + resultStatusCodeLine +
                          $"================STACK TRACE================\r\n" +
                          $"{ Environment.StackTrace }");

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

        protected BaseWebRequests()
        {
            clientPool = new HttpClientPool(GetBaseDomain());
        }

        #endregion
    }
}
