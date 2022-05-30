using NSL.RestExtensions.ResponseReaders;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NSL.RestExtensions
{

    public class WebRequestRepository : WebRequestRepository<DefaultRestResponseReader>
    {
    }

    public class WebRequestRepository<TConverter> : BaseWebRequests<TConverter>
        where TConverter : IRestResponseReader, new()
    {

        #region Utils

        public virtual void SetDefaultHeader(string name, string value) => clientPool.SetDefaultHeader(name, value);

        public void FreeClient(HttpClient client, BaseHttpRequestResult response = null) => FreeClient(client, response?.MessageResponse);

        public void FreeClient(HttpClient client, HttpResponseMessage response = null) => clientPool.FreeClient(client, response);

        public void FreeClient(HttpClient client) => clientPool.FreeClient(client);

        protected override async Task<HttpRequestResult> SafeRequest(string url, Action<HttpRequestMessage> request, bool dispose = false)
        {
            var client = await GetClient();

            var result = await SafeRequest(client, url, request);

            FreeClient(client, dispose ? result : null);

            return result;
        }

        protected override async Task<HttpRequestResult<TData>> SafeRequest<TData>(string url, Action<HttpRequestMessage> request = null, bool dispose = false)
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

            HttpRequestResult<TData> r = await ProcessBaseResponse<HttpRequestResult<TData>>(result) ?? new HttpRequestResult<TData>(result, await HttpContentConverter.GetContent<TData>(result, result.Content));

            FreeClient(client, result);

            return r;
        }

        protected override async Task<TResult> ProcessBaseResponse<TResult>(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
                return default;
            else if (response.StatusCode == HttpStatusCode.BadRequest)
                return new TResult()
                {
                    MessageResponse = response,
                    ErrorMessages = await HttpContentConverter.GetErrors(response, response.Content)
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
            //Unity has throw on error status code and form data content, cannot find any fixes
            catch (ObjectDisposedException ex)
            {
                result = new HttpResponseMessage((HttpStatusCode)BaseHttpRequestResult.ThrowStatusCode) { RequestMessage = request, Content = new StringContent(ex.ToString()) };
            }
#pragma warning disable CS0168 // Переменная объявлена, но не используется
            catch (HttpRequestException ex)
#pragma warning restore CS0168 // Переменная объявлена, но не используется
            {
                result = new HttpResponseMessage((HttpStatusCode)BaseHttpRequestResult.ThrowStatusCode) { RequestMessage = request, Content = new StringContent(ex.ToString()) };

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
            string resultContent = result.Content != null && result.Content is StreamContent sc ? await sc.ReadAsStringAsync() : null;
            await LogRequestResult(result, resultContent);

        }

        protected virtual async Task LogRequestResult(HttpResponseMessage result, string responseContent)
        {

        }

        protected WebRequestRepository()
        {
            clientPool = new HttpClientPool(() => GetBaseDomain());
        }

        #endregion

    }
}
