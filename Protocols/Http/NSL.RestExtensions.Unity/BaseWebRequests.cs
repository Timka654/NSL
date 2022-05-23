using NSL.SocketClient.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

namespace NSL.RestExtensions.Unity
{
    public class BaseWebRequests : NSL.RestExtensions.BaseWebRequests
    {


        #region Utils

        public void ClearAuth()
        {
            SetDefaultHeader("Authorization", null);
            Debug.Log("Logout. Setted empty auth header.");
        }

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

        protected override async Task LogRequestResult(HttpResponseMessage result, string responseContent)
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

        #endregion
    }
}
