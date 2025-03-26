using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Threading.Tasks;

namespace NSL.ASPNET.Mvc
{
    public abstract class IdResponseResult
    {
        public static IdResponseResult<TId> Ok<TId>(TId id)
            => new IdResponseResult<TId>(id);

        public static IdResponseResult<TData> NotFound<TData>()
            => NotFound<TData>("{...no_found}");

        public static IdResponseResult<TData> NotFound<TData>(string errorMessage)
            => Error<TData>(HttpStatusCode.NotFound, errorMessage);

        public static IdResponseResult<TData> InternalServerError<TData>()
            => StatusCode<TData>(HttpStatusCode.InternalServerError);

        public static IdResponseResult<TData> InternalServerError<TData>(string errorMessage)
            => Error<TData>(HttpStatusCode.InternalServerError, errorMessage);

        public static IdResponseResult<TData> Forbid<TData>()
            => new IdResponseResult<TData>((int)HttpStatusCode.Forbidden, default);

        public static IdResponseResult<TData> Forbid<TData>(string errorMessage)
            => Error<TData>(HttpStatusCode.NotFound, errorMessage);

        public static IdResponseResult<TData> Error<TData>(HttpStatusCode code, string errorMessage)
            => new IdResponseResult<TData>((int)code, new
            {
                Key = string.Empty,
                Value = (string[])[errorMessage]
            });

        public static IdResponseResult<TData> StatusCode<TData>(HttpStatusCode code)
            => new IdResponseResult<TData>((int)code, default);

        public static IdResponseResult<TData> ModelState<TData>(ControllerBase controller, HttpStatusCode code)
            => new IdResponseResult<TData>((int)code, ControllerResults.formatModelState(controller.ModelState));
    }

    public class IdResponseResult<TId> : DataResponseResult<object>
    {
        public IdResponseResult(TId id) : base(200, new { id })
        {
        }

        public IdResponseResult(int statusCode, object value) : base(statusCode,value)
        {
        }

    }
}
