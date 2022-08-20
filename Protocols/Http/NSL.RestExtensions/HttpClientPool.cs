using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.RestExtensions
{
    public abstract class HttpClientPool<TClient>
        where TClient : HttpClient
    {
        public const byte MaxParrallelRequestCount = byte.MaxValue;

        public static readonly TimeSpan defaultTimeout = TimeSpan.FromSeconds(60);

        private Func<string> baseUrl;
        private TimeSpan requestTimeout;

        private SemaphoreSlim locker = new SemaphoreSlim(MaxParrallelRequestCount);

        private System.Collections.Concurrent.ConcurrentDictionary<string, string> headerMap = new System.Collections.Concurrent.ConcurrentDictionary<string, string>();

        public HttpClientPool(Func<string> baseUrl, TimeSpan? requestTimeout = null)
        {
            this.baseUrl = baseUrl;
            this.requestTimeout = requestTimeout ?? defaultTimeout;
        }

        public async Task<TClient> GetClient(int timeout = 20000, string baseUrl = null, TimeSpan? requestTimeout = null, HttpClientHandler customHandler = null)
        {
            if (!await locker.WaitAsync(timeout))
                return default;

            var client = CreateClient(baseUrl ?? this.baseUrl(), requestTimeout ?? this.requestTimeout, customHandler ?? new HttpClientHandler()
            {
                AllowAutoRedirect = false
            });

            foreach (var item in headerMap)
            {
                client.DefaultRequestHeaders.Add(item.Key, item.Value);
            }

            return client;
        }
        protected abstract TClient CreateClient(string baseUrl, TimeSpan requestTimeout, HttpClientHandler handler);

        public void FreeClient(HttpClient client, HttpResponseMessage response = null)
        {
            response?.Dispose();
            client.Dispose();

            locker.Release();
        }

        public void SetDefaultHeader(string key, string value)
        {
            if (value == null)
            {
                headerMap.TryRemove(key, out var d);
                return;
            }

            if (headerMap.TryAdd(key, value) == false)
                headerMap[key] = value;
        }
    }
}
