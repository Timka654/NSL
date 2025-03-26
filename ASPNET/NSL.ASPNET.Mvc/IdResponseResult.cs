using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Threading.Tasks;

namespace NSL.ASPNET.Mvc
{
    public abstract class IdResponse
    {
        public static IdResponse<TId> Ok<TId>(TId id)
            => new IdResponse<TId>(id);

        public static IdResponse<TData> NotFound<TData>()
            => NotFound<TData>("{...no_found}");

        public static IdResponse<TData> NotFound<TData>(string errorMessage)
            => Error<TData>(HttpStatusCode.NotFound, errorMessage);

        public static IdResponse<TData> InternalServerError<TData>()
            => StatusCode<TData>(HttpStatusCode.InternalServerError);

        public static IdResponse<TData> InternalServerError<TData>(string errorMessage)
            => Error<TData>(HttpStatusCode.InternalServerError, errorMessage);

        public static IdResponse<TData> Forbid<TData>()
            => new IdResponse<TData>((int)HttpStatusCode.Forbidden, default);

        public static IdResponse<TData> Forbid<TData>(string errorMessage)
            => Error<TData>(HttpStatusCode.NotFound, errorMessage);

        public static IdResponse<TData> Error<TData>(HttpStatusCode code, string errorMessage)
            => new IdResponse<TData>((int)code, new
            {
                Key = string.Empty,
                Value = (string[])[errorMessage]
            });

        public static IdResponse<TData> StatusCode<TData>(HttpStatusCode code)
            => new IdResponse<TData>((int)code, default);

        public static IdResponse<TData> ModelState<TData>(ControllerBase controller, HttpStatusCode code)
            => new IdResponse<TData>((int)code, ControllerResults.formatModelState(controller.ModelState));
    }

    public class IdResponse<TId> : DataResponse<object>
    {
        public IdResponse(TId id) : base(200, new { id })
        {
        }

        public IdResponse(int statusCode, object value) : base(statusCode,value)
        {
        }

    }
}
