using NSL.HttpClient.HttpContent;
using NSL.HttpClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace NSL.HttpClient
{
    public static class HttpResponseExtensions
    {
        public static async Task<BaseResponse> ProcessResponseAsync(
            this Task<HttpResponseMessage> request,
            BaseHttpRequestOptions options = null)
            => await request.ProcessResponseAsync<BaseResponse>(options);

        public static Task<DataResponse<TData>> ProcessDataResponseAsync<TData>(
            this Task<HttpResponseMessage> request,
            BaseHttpRequestOptions options = null)
            => request.ProcessResponseAsync<DataResponse<TData>>(options);

        public static Task<DataListResponse<TData>> ProcessDataListResponseAsync<TData>(
            this Task<HttpResponseMessage> request,
            BaseHttpRequestOptions options = null)
            => request.ProcessResponseAsync<DataListResponse<TData>>(options);

        public static Task<IdResponse<TData>> ProcessIdResponseAsync<TData>(
            this Task<HttpResponseMessage> request,
            BaseHttpRequestOptions options = null)
            => request.ProcessResponseAsync<IdResponse<TData>>(options);

        public static async Task<TResult> ProcessResponseAsync<TResult>(this Task<HttpResponseMessage> request, BaseHttpRequestOptions options = null)
            where TResult : BaseResponse, new()
        {
            try
            {
                var response = await request;

                TResult result;

                if (!response.IsSuccessStatusCode)
                {
                    result = new TResult()
                    {
                        StatusCode = response.StatusCode,
                        Errors = await response.ReadErrorsAsync(options) ??
                        await response.ReadErrorsAsync(options, v2: false) ??
                        new Dictionary<string, List<HttpResponseErrorModel>>()
                    };

                    options?.Validator?.DisplayApiErrors(result);
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync();

                    if (!string.IsNullOrWhiteSpace(content))
                        result = JsonSerializer.Deserialize<TResult>(content, options?.JsonOptions ?? JsonHttpContent.BuildJsonOptions(null));
                    else
                        result = new TResult();

                    result.StatusCode = response.StatusCode;
                }

                await options.ResponsePostProcess(response, result);

                return result;
            }
            catch (Exception ex)
            {
                var exresp = new TResult();

                var type = BaseHttpRequestOptions.BaseHttpExceptionHandleResult.Throw;

                if (options != null)
                    type = options.ExceptionHandle(ex, options, exresp);
                else if (ex is HttpRequestException hre)
                {
#if UNITY
                    exresp.StatusCode = 0;
#else
                    exresp.StatusCode = hre.StatusCode ?? 0;
#endif
                    type = BaseHttpRequestOptions.BaseHttpExceptionHandleResult.Response;
                }
                else if (ex is OperationCanceledException)
                {
                    exresp.StatusCode = System.Net.HttpStatusCode.RequestTimeout;
                    type = BaseHttpRequestOptions.BaseHttpExceptionHandleResult.Response;
                }


                if (type == BaseHttpRequestOptions.BaseHttpExceptionHandleResult.Throw)
                    throw;


                await options.ResponsePostProcess(new HttpResponseMessage(System.Net.HttpStatusCode.RequestTimeout), exresp);

                return exresp;
            }
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

        public static async Task<Dictionary<string, List<HttpResponseErrorModel>>> ReadErrorsAsync(
            this HttpResponseMessage response,
            BaseHttpRequestOptions options = null,
            bool v2 = true)
        {
            Dictionary<string, List<HttpResponseErrorModel>>? result = default;

            if (response.Content?.Headers.ContentType?.MediaType != "application/json")
                return result;

            var content = await response.Content?.ReadAsStringAsync();

            if (string.IsNullOrEmpty(content))
                return result;

            try
            {
                if (v2)
                {
                    result = JsonSerializer.Deserialize<Dictionary<string, List<HttpResponseErrorModel>>>(content, options?.JsonOptions ?? JsonHttpContent.DefaultJsonOptions);
                }
                else
                {
                    var _result = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(content, options?.JsonOptions ?? JsonHttpContent.DefaultJsonOptions);

                    if (_result != null)
                        result = _result.ToDictionary(
                            x => x.Key,
                            x => new List<HttpResponseErrorModel>(
                                x.Value.Select(v => new HttpResponseErrorModel(v, Array.Empty<string>()))
                                )
                            );
                }

            }
            catch (JsonException)
            {
            }

            if (result == default)
                return result;

            foreach (var key in result)
            {
                for (int i = 0; i < key.Value.Count; i++)
                {
                    var val = key.Value[i];

                    if (val.Message.StartsWith('{') && val.Message.EndsWith('}') && val.Message.IndexOf("...") == 1)
                    {
                        val.Message = val.Message.Substring(4);

                        if (options?.ErrorPrefix != default)
                            val.Message = string.Join('_', options.ErrorPrefix, val.Message);

                        val.Message = $"{{{val.Message}}}";

                        if (options != null)
                            await options.ProcessMessageAsync(val);
                    }
                }
            }

            return result;
        }
    }
}
