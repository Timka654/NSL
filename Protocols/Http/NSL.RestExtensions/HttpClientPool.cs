using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.RestExtensions
{
    public class HttpClientPool
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

        public async Task<HttpClient> GetClient(int timeout = 200, string baseUrl = null, TimeSpan? requestTimeout = null, HttpClientHandler customHandler = null)
        {
            if (!await locker.WaitAsync(timeout))
                return default;


            var client = new HttpClient(customHandler ?? new HttpClientHandler()
            {
                AllowAutoRedirect = false
            })
            {
                BaseAddress = new Uri(baseUrl ?? this.baseUrl()),
                Timeout = requestTimeout ?? this.requestTimeout
            };

            foreach (var item in headerMap)
            {
                client.DefaultRequestHeaders.Add(item.Key, item.Value);
            }

            return client;
        }

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
