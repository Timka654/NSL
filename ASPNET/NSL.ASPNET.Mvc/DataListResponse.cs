using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net;

namespace NSL.ASPNET.Mvc
{
    public abstract class DataListResponse
    {
        public static DataListResponse<TData> Ok<TData>(IEnumerable<TData> data)
            => new DataListResponse<TData>(data);

        public static DataListResponse<TData> NotFound<TData>()
            => NotFound<TData>("{...no_found}");

        public static DataListResponse<TData> NotFound<TData>(string errorMessage)
            => Error<TData>(HttpStatusCode.NotFound, errorMessage);

        public static DataListResponse<TData> InternalServerError<TData>()
            => StatusCode<TData>(HttpStatusCode.InternalServerError);

        public static DataListResponse<TData> InternalServerError<TData>(string errorMessage)
            => Error<TData>(HttpStatusCode.InternalServerError, errorMessage);

        public static DataListResponse<TData> Forbid<TData>()
            => new DataListResponse<TData>((int)HttpStatusCode.Forbidden, default);

        public static DataListResponse<TData> Forbid<TData>(string errorMessage)
            => Error<TData>(HttpStatusCode.Forbidden, errorMessage);

        public static DataListResponse<TData> Error<TData>(HttpStatusCode code, string errorMessage)
            => new DataListResponse<TData>((int)code, new Dictionary<string, string[]>
            {
                { string.Empty, [errorMessage]}
            });

        public static DataListResponse<TData> StatusCode<TData>(HttpStatusCode code)
            => new DataListResponse<TData>((int)code, default);

        public static DataListResponse<TData> ModelState<TData>(ControllerBase controller, HttpStatusCode code)
            => new DataListResponse<TData>((int)code, ControllerResults.formatModelState(controller.ModelState));
    }
    public class DataListResponse<TData> : DataResponse<object>
    {
        public DataListResponse(IEnumerable<TData> data) : base(data) { }

        public DataListResponse(int statusCode, object value) : base(statusCode, value) { }
    }
}
