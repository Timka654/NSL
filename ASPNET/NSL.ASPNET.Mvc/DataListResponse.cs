using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace NSL.ASPNET.Mvc
{
    public abstract class DataListResponse
    {
        public static DataListResponse<TData> Ok<TData>(IEnumerable<TData> data)
            => new DataListResponse<TData>(data);

        public static DataListResponse<TData> Ok<TData>(IEnumerable<object> data)
            => new DataListResponse<TData>(data);

        public static DataListResponse<TData> NotFound<TData>(params string[] args)
            => NotFound<TData>("{...no_found}", args);

        public static DataListResponse<TData> NotFound<TData>(string errorMessage, params string[] args)
            => Error<TData>(HttpStatusCode.NotFound, errorMessage, args);

        public static DataListResponse<TData> InternalServerError<TData>()
            => StatusCode<TData>(HttpStatusCode.InternalServerError);

        public static DataListResponse<TData> InternalServerError<TData>(string errorMessage, params string[] args)
            => Error<TData>(HttpStatusCode.InternalServerError, errorMessage, args);

        public static DataListResponse<TData> Forbid<TData>()
            => new DataListResponse<TData>((int)HttpStatusCode.Forbidden, default);

        public static DataListResponse<TData> Forbid<TData>(string errorMessage, params string[] args)
            => Error<TData>(HttpStatusCode.Forbidden, errorMessage, args);

        public static DataListResponse<TData> Error<TData>(HttpStatusCode code, string errorMessage, params string[] args)
            => new DataListResponse<TData>((int)code, new Dictionary<string, HttpResponseErrorModel[]>
            {
                { string.Empty, [new HttpResponseErrorModel(errorMessage, args)]}
            });

        public static DataListResponse<TData> Error<TData>(HttpStatusCode code, params IEnumerable<(string key, string errorMessage, string[] args)> errors)
            => new DataListResponse<TData>((int)code, errors
                .GroupBy(x => x.key)
                .ToDictionary(
                x => x.Key,
                x => x.Select(x => new HttpResponseErrorModel(x.errorMessage, x.args)).ToArray()));

        public static DataListResponse<TData> BadRequest<TData>(params IEnumerable<(string key, string errorMessage, string[] args)> errors)
            => Error<TData>(HttpStatusCode.BadRequest, errors);

        public new static DataListResponse<TData> StatusCode<TData>(HttpStatusCode code)
            => new DataListResponse<TData>((int)code, default);

        public static DataListResponse<TData> ModelState<TData>(ControllerBase controller, HttpStatusCode code)
            => new DataListResponse<TData>((int)code, ControllerResults.formatModelState(controller.ModelState));
    }

    public class DataListResponse<TData> : DataResponse<object>
    {
        public DataListResponse(IEnumerable<TData> data) : this(200, new { data }) { }

        public DataListResponse(IEnumerable<object> data) : this(200, new { data }) { }

        public DataListResponse(int statusCode, object value) : base(statusCode, value) { }
    }
}
