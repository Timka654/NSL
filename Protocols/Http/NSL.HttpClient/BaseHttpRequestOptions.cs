using NSL.HttpClient.HttpContent;
using NSL.HttpClient.Models;
using NSL.HttpClient.Validators;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.HttpClient
{
    public class BaseHttpRequestOptions
    {
        public enum BaseHttpExceptionHandleResult
        {
            Throw,
            Response
        }

        public delegate BaseHttpExceptionHandleResult ExceptionHandleDelegate(Exception ex, BaseHttpRequestOptions options, BaseResponse? response);

        public delegate System.Net.Http.HttpClient RequestClientBuildHandler(System.Net.Http.HttpClient client);

        /// <summary>
        /// Validator ref for display errors
        /// </summary>
        public IHttpResponseContentValidator Validator { get; set; }

        public RequestClientBuildHandler ClientBuilder { get; set; }

        public IHttpIOProcessor IOProcessor { get; set; } = DefaultHttpIOProcessor.Instance;

        public CancellationToken CancellationToken { get; set; } = CancellationToken.None;

        public Dictionary<string, object> ObjectBag { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// 
        /// </summary>
        public string ErrorPrefix { get; set; }

        public JsonSerializerOptions JsonOptions { get; set; }

        public Func<HttpResponseErrorModel, Task> ProcessMessageAsync { get; set; } = v => Task.CompletedTask;

        public ExceptionHandleDelegate ExceptionHandle { get; set; } = BaseExceptionHandle;

        public static BaseHttpExceptionHandleResult BaseExceptionHandle(Exception ex, BaseHttpRequestOptions options, BaseResponse? response)
        {
            if (ex is OperationCanceledException)
            {
                response.StatusCode = System.Net.HttpStatusCode.RequestTimeout;

                return BaseHttpExceptionHandleResult.Response;
            }
            if (ex is HttpRequestException hre)
            {
#if UNITY
                response.StatusCode = 0;
#else
                response.StatusCode = hre.StatusCode ?? 0;
#endif

                return BaseHttpExceptionHandleResult.Response;
            }

            return BaseHttpExceptionHandleResult.Throw;
        }

        public BaseHttpRequestOptions Clone()
        {
            var r = MemberwiseClone() as BaseHttpRequestOptions;

            r.ObjectBag = new Dictionary<string, object>(ObjectBag);

            return r;
        }


        public static BaseHttpRequestOptions Create(IHttpResponseContentValidator validator) => new BaseHttpRequestOptions() { Validator = validator };

        public static BaseHttpRequestOptions Create(string errorPrefix) => new BaseHttpRequestOptions() { ErrorPrefix = errorPrefix };

        public static BaseHttpRequestOptions Create(JsonSerializerOptions jsonOptions) => new BaseHttpRequestOptions() { JsonOptions = jsonOptions };

        public static BaseHttpRequestOptions Create(RequestClientBuildHandler clientHandler) => new BaseHttpRequestOptions() { ClientBuilder = clientHandler };

        public static BaseHttpRequestOptions Create(CancellationToken cancellationToken) => new BaseHttpRequestOptions() { CancellationToken = cancellationToken };

        public static BaseHttpRequestOptions Create(ExceptionHandleDelegate exceptionHandle) => new BaseHttpRequestOptions() { ExceptionHandle = exceptionHandle };

        public static BaseHttpRequestOptions Create(IHttpIOProcessor ioProcessor) => new BaseHttpRequestOptions() { IOProcessor = ioProcessor };

        public static BaseHttpRequestOptions Create() => new BaseHttpRequestOptions();

        public BaseHttpRequestOptions WithValidator(IHttpResponseContentValidator validator, bool clone = false)
        {
            if (clone)
                return Clone().WithValidator(validator);

            Validator = validator;
            return this;
        }

        public BaseHttpRequestOptions WithErrorPrefix(string errorPrefix, bool clone = false)
        {
            if (clone)
                return Clone().WithErrorPrefix(errorPrefix);

            ErrorPrefix = errorPrefix;
            return this;
        }

        public BaseHttpRequestOptions WithJsonOptions(JsonSerializerOptions jsonOptions, bool clone = false)
        {
            if (clone)
                return Clone().WithJsonOptions(jsonOptions);

            JsonOptions = jsonOptions;
            return this;
        }

        public BaseHttpRequestOptions WithClientBuilder(RequestClientBuildHandler clientBuilder, bool clone = false)
        {
            if (clone)
                return Clone().WithClientBuilder(clientBuilder);

            ClientBuilder = clientBuilder;
            return this;
        }

        public BaseHttpRequestOptions WithCancellationToken(CancellationToken cancellationToken, bool clone = false)
        {
            if (clone)
                return Clone().WithCancellationToken(cancellationToken);

            CancellationToken = cancellationToken;
            return this;
        }

        public BaseHttpRequestOptions WithExceptionHandle(ExceptionHandleDelegate exceptionHandle, bool clone = false)
        {
            if (clone)
                return Clone().WithExceptionHandle(exceptionHandle);

            ExceptionHandle = exceptionHandle;
            return this;
        }

        public BaseHttpRequestOptions WithIOProcessor(IHttpIOProcessor ioProcessor, bool clone = false)
        {
            if (clone)
                return Clone().WithIOProcessor(ioProcessor);

            IOProcessor = ioProcessor;
            return this;
        }

        public BaseHttpRequestOptions WithObjectValue(string key, object value, bool clone = false)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (clone)
                return Clone().WithObjectValue(key, value);

            ObjectBag[key] = value;
            return this;
        }

        public bool TryGetObjectValue(string key, out object value)
        {
            return ObjectBag.TryGetValue(key, out value);
        }

        public bool TryGetObjectValue<TValue>(string key, out TValue value)
        {
            if (ObjectBag.TryGetValue(key, out var _value))
            {
                value = (TValue)_value;
                return true;
            }

            value = default;
            return false;
        }
    }
}
