using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace NSL.RestExtensions.Unity
{
    public class UnityHttpClientPool : HttpClientPool<UnityHttpClient>
    {
        public UnityHttpClientPool(Func<string> baseUrl, TimeSpan? requestTimeout = null) : base(baseUrl, requestTimeout)
        {
        }

        protected override UnityHttpClient CreateClient(string baseUrl, TimeSpan requestTimeout, HttpClientHandler handler)
        {
            return new UnityHttpClient()
            {
                BaseAddress = new Uri(baseUrl),
                Timeout = requestTimeout
            };
        }
    }
}
