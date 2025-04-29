using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace NSL.ASPNET.Mvc
{
    public abstract class IdResponse
    {
        public static IdResponse<TId> Ok<TId>(TId id)
            => new IdResponse<TId>(id);

        public static IdResponse<TId> NotFound<TId>()
            => NotFound<TId>("{...no_found}");

        public static IdResponse<TId> NotFound<TId>(string errorMessage)
            => Error<TId>(HttpStatusCode.NotFound, errorMessage);

        public static IdResponse<TId> InternalServerError<TId>()
            => StatusCode<TId>(HttpStatusCode.InternalServerError);

        public static IdResponse<TId> InternalServerError<TId>(string errorMessage)
            => Error<TId>(HttpStatusCode.InternalServerError, errorMessage);

        public static IdResponse<TId> Forbid<TId>()
            => new IdResponse<TId>((int)HttpStatusCode.Forbidden, default);

        public static IdResponse<TId> Forbid<TId>(string errorMessage)
            => Error<TId>(HttpStatusCode.Forbidden, errorMessage);

        public static IdResponse<TId> Error<TId>(HttpStatusCode code, string errorMessage)
            => new IdResponse<TId>((int)code, new Dictionary<string, string[]>
            {
                { string.Empty, [errorMessage]}
            });

        public static IdResponse<TId> StatusCode<TId>(HttpStatusCode code)
            => new IdResponse<TId>((int)code, default);

        public static IdResponse<TId> ModelState<TId>(ControllerBase controller, HttpStatusCode code)
            => new IdResponse<TId>((int)code, ControllerResults.formatModelState(controller.ModelState));
    }

    public class IdResponse<TId> : DataResponse<object>
    {
        public IdResponse(TId id) : this(200, new { id })
        {
        }

        public IdResponse(int statusCode, object value) : base(statusCode, value)
        {
        }

    }
}
