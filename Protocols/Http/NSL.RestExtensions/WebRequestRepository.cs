using NSL.RestExtensions.RESTContentProcessors;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.RestExtensions
{
    public class WebRequestRepository : WebRequestRepository<DefaultRestContentProcessor>
    {
    }

    public class WebRequestRepository<TConverter> : WebRequestRepository<BaseHttpClientPool, HttpClient, TConverter>
        where TConverter : IRestContentProcessor, new()
    {
        public WebRequestRepository() : base()
        {
            clientPool = new BaseHttpClientPool(() => GetBaseDomain());
        }
    }

    public class WebRequestRepository<TClientPool, TClient, TConverter> : BaseWebRequests<TConverter>
        where TConverter : IRestContentProcessor, new()
        where TClient : HttpClient
        where TClientPool : HttpClientPool<TClient>
    {

        #region Utils

        public virtual void SetDefaultHeader(string name, string value) => clientPool.SetDefaultHeader(name, value);

        public void FreeClient(TClient client, BaseHttpRequestResult response = null) => FreeClient(client, response?.MessageResponse);

        public void FreeClient(TClient client, HttpResponseMessage response = null) => clientPool.FreeClient(client, response);

        public void FreeClient(TClient client) => clientPool.FreeClient(client);

        protected override async Task<HttpRequestResult> SafeRequest(string url, Action<HttpRequestMessage> request, bool dispose = false, CancellationToken? cancellationToken = null)
        {
            var client = await GetClient();

            var result = await SafeRequest(client, url, request, cancellationToken);

            FreeClient(client, dispose ? result : null);

            return result;
        }

        protected override async Task<HttpRequestResult<TData>> SafeRequest<TData>(string url, Action<HttpRequestMessage> request = null, bool dispose = false, CancellationToken? cancellationToken = null)
        {
            var client = await GetClient();

            var result = await SafeRequest<TData>(client, url, request, cancellationToken);

            FreeClient(client, dispose ? result : null);

            return result;
        }

        protected async Task<HttpRequestResult> SafeRequest(TClient client, string url, Action<HttpRequestMessage> request, CancellationToken? cancellationToken = null)
        {
            var message = new HttpRequestMessage(GetHttpMethod(), url);

            request.Invoke(message);

            return await SafeRequest(client, message, cancellationToken);
        }

        protected async Task<HttpRequestResult<TData>> SafeRequest<TData>(TClient client, string url, Action<HttpRequestMessage> request = null, CancellationToken? cancellationToken = null)
        {
            var message = new HttpRequestMessage(GetHttpMethod(), url);

            if (request != null)
                request.Invoke(message);

            return await SafeRequest<TData>(client, message, cancellationToken);
        }

        protected async Task<HttpRequestResult> SafeRequest(TClient client, HttpRequestMessage request, CancellationToken? cancellationToken = null)
        {
            var result = await SafeSendRequest(client, request, cancellationToken);

            await LogRequestResult(result);

            HttpRequestResult r = await ProcessBaseResponse<HttpRequestResult>(result) ?? new HttpRequestResult(result);

            FreeClient(client, result);

            return r;
        }

        protected async Task<HttpRequestResult<TData>> SafeRequest<TData>(TClient client, HttpRequestMessage request, CancellationToken? cancellationToken = null)
        {
            var result = await SafeSendRequest(client, request, cancellationToken);

            await LogRequestResult(result);

            HttpRequestResult<TData> r = await ProcessBaseResponse<HttpRequestResult<TData>>(result) ?? new HttpRequestResult<TData>(result, (result.Content == null ? default : await HttpContentConverter.GetContent<TData>(result, result.Content)));
            
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

        protected virtual async Task<HttpResponseMessage> SafeSendRequest(TClient client, HttpRequestMessage request, CancellationToken? cancellationToken = null)
        {
            HttpResponseMessage result = default;

            try
            {
                result = await client.SendAsync(request, cancellationToken ?? CancellationToken.None);
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

        protected TClientPool clientPool;

        protected async Task<TClient> GetClient()
        {
            return await clientPool.GetClient(requestTimeout: TimeSpan.FromMilliseconds(GetTimeout()));
        }

        protected virtual async Task LogRequestResult(HttpResponseMessage result)
        {
            string resultContent = result.Content != null && result.Content is StreamContent sc ? await sc.ReadAsStringAsync() : null;
            await LogRequestResult(result, resultContent);

        }

        protected virtual Task LogRequestResult(HttpResponseMessage result, string responseContent)
        {
            return Task.CompletedTask;
        }

        #endregion

        public WebRequestRepository()
        {

        }

    }
}
