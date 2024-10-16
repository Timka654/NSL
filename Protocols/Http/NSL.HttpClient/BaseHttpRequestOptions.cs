using NSL.HttpClient.Validators;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NSL.HttpClient
{
    public class BaseHttpRequestOptions
    {
        public delegate System.Net.Http.HttpClient RequestClientBuildHandler(System.Net.Http.HttpClient client);

        /// <summary>
        /// Validator ref for display errors
        /// </summary>
        public IHttpResponseContentValidator Validator { get; set; }

        public RequestClientBuildHandler ClientHandler { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ErrorPrefix { get; set; }

        public JsonSerializerOptions JsonOptions { get; set; }

        public Func<string, Task<string>> ProcessMessage { get; set; } = v => Task.FromResult(v);

        public BaseHttpRequestOptions Clone()
            => MemberwiseClone() as BaseHttpRequestOptions;

        public static BaseHttpRequestOptions Create(IHttpResponseContentValidator validator) => new BaseHttpRequestOptions() { Validator = validator };

        public static BaseHttpRequestOptions Create(string errorPrefix) => new BaseHttpRequestOptions() { ErrorPrefix = errorPrefix };

        public static BaseHttpRequestOptions Create(JsonSerializerOptions jsonOptions) => new BaseHttpRequestOptions() { JsonOptions = jsonOptions };

        public static BaseHttpRequestOptions Create(RequestClientBuildHandler clientHandler) => new BaseHttpRequestOptions() { ClientHandler = clientHandler };

        public static BaseHttpRequestOptions Create() => new BaseHttpRequestOptions();



        public BaseHttpRequestOptions WithValidator(IHttpResponseContentValidator validator)
        {
            Validator = validator;
            return this;
        }

        public BaseHttpRequestOptions WithErrorPrefix(string errorPrefix)
        {
            ErrorPrefix = errorPrefix;
            return this;
        }

        public BaseHttpRequestOptions WithJsonOptions(JsonSerializerOptions jsonOptions)
        {
            JsonOptions = jsonOptions;
            return this;
        }

        public BaseHttpRequestOptions WithClientHandler(RequestClientBuildHandler clientHandler)
        {
            ClientHandler = clientHandler;
            return this;
        }

    }

    public static class RequestOptionsExtensions
    {
        public static System.Net.Http.HttpClient FillClientOptions(this System.Net.Http.HttpClient client, BaseHttpRequestOptions options)
        {
            if (options?.ClientHandler != null)
                return options.ClientHandler(client);

            return client;
        }
    }
}
