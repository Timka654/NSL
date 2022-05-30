﻿using NSL.RestExtensions.ResponseReaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NSL.RestExtensions
{
    public delegate void WebResponseDelegate(HttpRequestResult request);

    public delegate void WebResponseDelegate<TResult>(HttpRequestResult<TResult> request);

    //public delegate void WebResponseDelegate<TRequest>(TRequest requestData,HttpRequestResult request);

    public delegate void WebResponseDelegate<TRequest, TResult>(TRequest requestData, HttpRequestResult<TResult> request);


    public abstract class BaseWebRequests<TConverter>
        where TConverter : IRestResponseReader, new()
    {
        public virtual int GetTimeout() => 10000;

        public virtual string GetBaseDomain() => "http://127.0.0.1/";

        public virtual bool GetLogging() => true;

        public virtual HttpMethod GetHttpMethod() => HttpMethod.Post;

        protected TConverter HttpContentConverter { get; } = new TConverter();


        #region Utils

        protected abstract Task<HttpRequestResult> SafeRequest(string url, Action<HttpRequestMessage> request, bool dispose = false);

        protected abstract Task<HttpRequestResult<TData>> SafeRequest<TData>(string url, Action<HttpRequestMessage> request = null, bool dispose = false);

        protected abstract Task<TResult> ProcessBaseResponse<TResult>(HttpResponseMessage response)
            where TResult : BaseHttpRequestResult, new();

        protected BaseWebRequests()
        {
        }

        #endregion
    }
}
