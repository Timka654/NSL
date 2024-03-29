﻿using System;
using System.Net.Http;

namespace NSL.RestExtensions
{
    public class BaseHttpClientPool : HttpClientPool<HttpClient>
    {
        public BaseHttpClientPool(Func<string> baseUrl, TimeSpan? requestTimeout = null) : base(baseUrl, requestTimeout)
        {
        }

        protected override HttpClient CreateClient(string baseUrl, TimeSpan requestTimeout, HttpClientHandler handler)
        {
            return new HttpClient(handler)
            {
                BaseAddress = new Uri(baseUrl),
                Timeout = requestTimeout
            };
        }
    }
}
