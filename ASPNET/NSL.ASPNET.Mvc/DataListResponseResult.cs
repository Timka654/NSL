using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.Net;

namespace NSL.ASPNET.Mvc
{
    public abstract class DataListResponseResult
    {
        public static DataListResponseResult<TData> Ok<TData>(IEnumerable<TData> data)
            => new DataListResponseResult<TData>(data);

        public static DataListResponseResult<TData> NotFound<TData>()
            => NotFound<TData>("{...no_found}");

        public static DataListResponseResult<TData> NotFound<TData>(string errorMessage)
            => Error<TData>(HttpStatusCode.NotFound, errorMessage);

        public static DataListResponseResult<TData> InternalServerError<TData>()
            => StatusCode<TData>(HttpStatusCode.InternalServerError);

        public static DataListResponseResult<TData> InternalServerError<TData>(string errorMessage)
            => Error<TData>(HttpStatusCode.InternalServerError, errorMessage);

        public static DataListResponseResult<TData> Forbid<TData>()
            => new DataListResponseResult<TData>((int)HttpStatusCode.Forbidden, default);

        public static DataListResponseResult<TData> Forbid<TData>(string errorMessage)
            => Error<TData>(HttpStatusCode.NotFound, errorMessage);

        public static DataListResponseResult<TData> Error<TData>(HttpStatusCode code, string errorMessage)
            => new DataListResponseResult<TData>((int)code, new
            {
                Key = string.Empty,
                Value = (string[])[errorMessage]
            });

        public static DataListResponseResult<TData> StatusCode<TData>(HttpStatusCode code)
            => new DataListResponseResult<TData>((int)code, default);

        public static DataListResponseResult<TData> ModelState<TData>(ControllerBase controller, HttpStatusCode code)
            => new DataListResponseResult<TData>((int)code, ControllerResults.formatModelState(controller.ModelState));
    }

    public class DataListResponseResult<TData> : DataResponseResult<object>
    {
        public DataListResponseResult(IEnumerable<TData> value) : base(200, value) { }

        public DataListResponseResult(int statusCode, object value) : base(statusCode, value) { }
    }
}
