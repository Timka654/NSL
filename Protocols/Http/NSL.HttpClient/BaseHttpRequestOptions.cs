using NSL.HttpClient.Validators;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DevExtensions.Blazor.Http
{
    public class BaseHttpRequestOptions
    {
        /// <summary>
        /// Validator ref for display errors
        /// </summary>
        public IHttpResponseContentValidator? Validator { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string? ErrorPrefix { get; set; }

        public JsonSerializerOptions? JsonOptions { get; set; }

        public Func<string, Task<string>> ProcessMessage { get; set; } = v => Task.FromResult(v);

        public BaseHttpRequestOptions Clone()
            => this.MemberwiseClone() as BaseHttpRequestOptions;

        public static BaseHttpRequestOptions Create(IHttpResponseContentValidator validator) => new BaseHttpRequestOptions() { Validator = validator };

        public static BaseHttpRequestOptions Create(string errorPrefix) => new BaseHttpRequestOptions() { ErrorPrefix = errorPrefix };

        public static BaseHttpRequestOptions Create(JsonSerializerOptions jsonOptions) => new BaseHttpRequestOptions() { JsonOptions = jsonOptions };

        public static BaseHttpRequestOptions Create() => new BaseHttpRequestOptions();



        public BaseHttpRequestOptions WithValidator(IHttpResponseContentValidator validator)
        {
            this.Validator = validator;
            return this;
        }

        public BaseHttpRequestOptions WithErrorPrefix(string errorPrefix)
        {
            this.ErrorPrefix = errorPrefix;
            return this;
        }

        public BaseHttpRequestOptions WithJsonOptions(JsonSerializerOptions jsonOptions)
        {
            this.JsonOptions = jsonOptions;
            return this;
        }

    }
}
