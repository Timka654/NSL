using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace NSL.ASPNET.Mvc
{
    public abstract class IdResponse
    {
        public static IdResponse<TId> Ok<TId>(TId id)
            => new IdResponse<TId>(id);

        public static IdResponse<TId> Ok<TId>(object id)
            => new IdResponse<TId>(id);

        public static IdResponse<TData> NotFound<TData>(params string[] args)
            => NotFound<TData>("{...no_found}", args);

        public static IdResponse<TData> NotFound<TData>(string errorMessage, params string[] args)
            => Error<TData>(HttpStatusCode.NotFound, errorMessage, args);

        public static IdResponse<TData> InternalServerError<TData>()
            => StatusCode<TData>(HttpStatusCode.InternalServerError);

        public static IdResponse<TData> InternalServerError<TData>(string errorMessage, params string[] args)
            => Error<TData>(HttpStatusCode.InternalServerError, errorMessage, args);

        public static IdResponse<TData> Forbid<TData>()
            => new IdResponse<TData>((int)HttpStatusCode.Forbidden, default);

        public static IdResponse<TData> Forbid<TData>(string errorMessage, params string[] args)
            => Error<TData>(HttpStatusCode.Forbidden, errorMessage, args);

        public static IdResponse<TData> Error<TData>(HttpStatusCode code, string errorMessage, params string[] args)
            => new IdResponse<TData>((int)code, new Dictionary<string, HttpResponseErrorModel[]>
            {
                { string.Empty, [new HttpResponseErrorModel(errorMessage, args)]}
            });

        public static IdResponse<TData> Error<TData>(HttpStatusCode code, params IEnumerable<(string key, string errorMessage, string[] args)> errors)
            => new IdResponse<TData>((int)code, errors
                .GroupBy(x => x.key)
                .ToDictionary(
                x => x.Key,
                x => x.Select(x => new HttpResponseErrorModel(x.errorMessage, x.args)).ToArray()));

        public static IdResponse<TData> BadRequest<TData>(params IEnumerable<(string key, string errorMessage, string[] args)> errors)
            => Error<TData>(HttpStatusCode.BadRequest, errors);

        public new static IdResponse<TData> StatusCode<TData>(HttpStatusCode code)
            => new IdResponse<TData>((int)code, default);

        public static IdResponse<TData> ModelState<TData>(ControllerBase controller, HttpStatusCode code)
            => new IdResponse<TData>((int)code, ControllerResults.formatModelState(controller.ModelState));
    }

    public class IdResponse<TId> : DataResponse<object>
    {
        public IdResponse(TId id) : this(200, new { id })
        {
        }

        public IdResponse(object id) : this(200, new { id })
        {
        }

        public IdResponse(int statusCode, object value) : base(statusCode, value)
        {
        }

    }
}
