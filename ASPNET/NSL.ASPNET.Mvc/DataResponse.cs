using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace NSL.ASPNET.Mvc
{
    public abstract class DataResponse
    {
        public static DataResponse<TData> Ok<TData>(TData data)
            => new DataResponse<TData>(data);

        public static DataResponse<TData> Ok<TData>(object data)
            => new DataResponse<TData>(data);

        public static DataResponse<TData> NotFound<TData>(params string[] args)
            => NotFound<TData>("{...no_found}", args);

        public static DataResponse<TData> NotFound<TData>(string errorMessage, params string[] args)
            => Error<TData>(HttpStatusCode.NotFound, errorMessage, args);

        public static DataResponse<TData> InternalServerError<TData>()
            => StatusCode<TData>(HttpStatusCode.InternalServerError);

        public static DataResponse<TData> InternalServerError<TData>(string errorMessage, params string[] args)
            => Error<TData>(HttpStatusCode.InternalServerError, errorMessage, args);

        public static DataResponse<TData> Forbid<TData>()
            => new DataResponse<TData>((int)HttpStatusCode.Forbidden, default);

        public static DataResponse<TData> Forbid<TData>(string errorMessage, params string[] args)
            => Error<TData>(HttpStatusCode.Forbidden, errorMessage, args);

        public static DataResponse<TData> Error<TData>(HttpStatusCode code, string errorMessage, params string[] args)
            => new DataResponse<TData>((int)code, new Dictionary<string, HttpResponseErrorModel[]>
            {
                { string.Empty, [new HttpResponseErrorModel(errorMessage, args)]}
            });

        public static DataResponse<TData> Error<TData>(HttpStatusCode code, params IEnumerable<(string key, string errorMessage, string[] args)> errors)
            => new DataResponse<TData>((int)code, errors
                .GroupBy(x => x.key)
                .ToDictionary(
                x => x.Key,
                x => x.Select(x => new HttpResponseErrorModel(x.errorMessage, x.args)).ToArray()));

        public static DataResponse<TData> BadRequest<TData>(params IEnumerable<(string key, string errorMessage, string[] args)> errors)
            => Error<TData>(HttpStatusCode.BadRequest, errors);

        public new static DataResponse<TData> StatusCode<TData>(HttpStatusCode code)
            => new DataResponse<TData>((int)code, default);

        public static DataResponse<TData> ModelState<TData>(ControllerBase controller, HttpStatusCode code)
            => new DataResponse<TData>((int)code, ControllerResults.formatModelState(controller.ModelState));
    }

    public class DataResponse<TData> : ObjectResult
    {
        public override Task ExecuteResultAsync(ActionContext context)
        {
            if (StatusCode != 200 && context.ModelState.Count > 0)
                Value = ControllerResults.formatModelState(context.ModelState);

            return base.ExecuteResultAsync(context);
        }

        public DataResponse(TData data) : this(200, new { data })
        {
        }

        public DataResponse(object data) : this(200, new { data })
        {
        }

        public DataResponse(int statusCode, object value) : base(value)
        {
            this.StatusCode = statusCode;
        }
    }
}
