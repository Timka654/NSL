using DevExtensions.Blazor.Http.HttpContent;
using DevExtensions.Blazor.Http.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DevExtensions.Blazor.Http
{
    public static class HttpResponseExtensions
    {
        public static async Task<BaseResponse> ProcessResponseAsync(
            this Task<HttpResponseMessage> request,
            BaseHttpRequestOptions? options = null)
            => await request.ProcessResponseAsync<BaseResponse>(options);

        public static Task<DataResponse<TData>> ProcessDataResponseAsync<TData>(
            this Task<HttpResponseMessage> request,
            BaseHttpRequestOptions? options = null)
            => request.ProcessResponseAsync<DataResponse<TData>>(options);

        public static Task<DataListResponse<TData>> ProcessDataListResponseAsync<TData>(
            this Task<HttpResponseMessage> request,
            BaseHttpRequestOptions? options = null)
            => request.ProcessResponseAsync<DataListResponse<TData>>(options);

        public static Task<IdResponse<TData>> ProcessIdResponseAsync<TData>(
            this Task<HttpResponseMessage> request,
            BaseHttpRequestOptions? options = null)
            => request.ProcessResponseAsync<IdResponse<TData>>(options);

        public static async Task<TResult> ProcessResponseAsync<TResult>(this Task<HttpResponseMessage> request, BaseHttpRequestOptions? options = null)
            where TResult : BaseResponse, new()
        {
            var response = await request;

            TResult result;

            if (!response.IsSuccessStatusCode)
            {
                result = new TResult()
                {
                    StatusCode = response.StatusCode,
                    Errors = await response.ReadErrorsAsync(options)
                };

                options?.Validator?.DisplayApiErrors(result);
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();

                if (!string.IsNullOrWhiteSpace(content))
                    result = JsonSerializer.Deserialize<TResult>(content, options?.JsonOptions ?? JsonHttpContent.DefaultJsonOptions);
                else
                    result = new TResult();

                result.StatusCode = response.StatusCode;
            }

            return result;
        }

        public static async Task<TResult> ProcessBaseHttpResponseAsync<TResult>(this Task<HttpResponseMessage> request, Func<TResult, Task> postProcessing = null)
            where TResult : BaseHttpResponse, new()
        {
            var response = await request;

            var result = new TResult()
            {
                StatusCode = response.StatusCode,
                Response = response
            };

            if (result.IsSuccess)
            {
                var content = await response.Content.ReadAsStringAsync();

                if (!string.IsNullOrWhiteSpace(content))
                    result.Content = content;
            }

            if (postProcessing != null)
                await postProcessing(result);

            return result;
        }

        public static async Task<Dictionary<string, List<string>>?> ReadErrorsAsync(
            this HttpResponseMessage response,
            BaseHttpRequestOptions? options = null)
        {
            //if (response.StatusCode != System.Net.HttpStatusCode.BadRequest)
            //    return default;

            var content = await response.Content?.ReadAsStringAsync();

            if (string.IsNullOrEmpty(content))
                return new Dictionary<string, List<string>>();

            var result = JsonSerializer.Deserialize<Dictionary<string, List<string>>?>(content, options?.JsonOptions ?? JsonHttpContent.DefaultJsonOptions);

            if (result == null)
                return new Dictionary<string, List<string>>();

            foreach (var key in result)
            {
                for (int i = 0; i < key.Value.Count; i++)
                {
                    var val = key.Value[i];

                    if (val.StartsWith('{') && val.EndsWith('}') && val.IndexOf("...") == 1)
                    {
                        if (options?.ErrorPrefix == default)
                        {
                            val = $"{{{val.Substring(4)}";
                        }
                        else
                            val = $"{{{string.Join('_', options.ErrorPrefix, val.Substring(4))}";

                        if (options != null)
                            key.Value[i] = await options.ProcessMessage(val);
                    }
                }
            }

            return result;
        }
    }
}
