﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace NSL.RestExtensions
{
    public abstract class BaseHttpRequestResult : IDisposable
    {
        public const int ThrowStatusCode = 499;

        public const string DefaultJsonMimeType = "application/json";
        public const string DefaultXmlMimeType = "application/xml";

        public HttpResponseMessage MessageResponse { get; set; }

        public Dictionary<string, List<string>> ErrorMessages { get; set; } = new Dictionary<string, List<string>>();

        public BaseHttpRequestResult(HttpResponseMessage response)
        {
            MessageResponse = response;
        }

        public BaseHttpRequestResult(HttpResponseMessage response, Dictionary<string, List<string>> messages) : this(response)
        {
            ErrorMessages = messages;
        }

        public BaseHttpRequestResult()
        {

        }

        public void Dispose()
        {
            MessageResponse.Dispose();
        }

        public bool IsSuccess => MessageResponse.IsSuccessStatusCode;

        public bool IsBadRequest => MessageResponse.StatusCode == System.Net.HttpStatusCode.BadRequest;

        public IEnumerable<string> GetErrorMessages(string key = "")
        {
            ErrorMessages.TryGetValue(key, out var result);

            return result ?? new List<string>();
        }

        public override string ToString()
            => $"Status code: {MessageResponse?.StatusCode ?? (HttpStatusCode)ThrowStatusCode}({(int?)MessageResponse?.StatusCode ?? ThrowStatusCode}){Environment.NewLine}Errors:{Environment.NewLine}{string.Join(Environment.NewLine, ErrorMessages.SelectMany(x => x.Value))}";
    }

    public class HttpRequestResult<TData> : BaseHttpRequestResult
    {
        public TData Data { get; set; }

        public HttpRequestResult(HttpResponseMessage response, TData data) : base(response)
        {
            Data = data;
        }

        public HttpRequestResult(HttpResponseMessage response) : base(response)
        {
        }

        public HttpRequestResult(HttpResponseMessage response, Dictionary<string, List<string>> messages) : base(response, messages)
        {
        }

        public HttpRequestResult()
        {
        }
    }

    public class HttpRequestResult : BaseHttpRequestResult
    {
        public HttpRequestResult()
        {
        }

        public HttpRequestResult(HttpResponseMessage response) : base(response)
        {
        }

        public HttpRequestResult(HttpResponseMessage response, Dictionary<string, List<string>> messages) : base(response, messages)
        {
        }
    }
}
