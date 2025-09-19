using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace NSL.ASPNET.Mvc
{

    public class BaseResponse : ObjectResult
    {
        public override Task ExecuteResultAsync(ActionContext context)
        {
            if (base.StatusCode != 200 && context.ModelState.Count > 0)
                Value = ControllerResults.formatModelState(context.ModelState);

            return base.ExecuteResultAsync(context);
        }

        public BaseResponse() : this(200, null)
        {
        }

        public BaseResponse(int statusCode, object value) : base(value)
        {
            base.StatusCode = statusCode;
        }

        public static BaseResponse Ok()
            => new BaseResponse();

        public static BaseResponse NotFound(params string[] args)
            => NotFound("{...no_found}", args);

        public static BaseResponse NotFound(string errorMessage, params string[] args)
            => Error(HttpStatusCode.NotFound, errorMessage, args);

        public static BaseResponse InternalServerError()
            => StatusCode(HttpStatusCode.InternalServerError);

        public static BaseResponse InternalServerError(string errorMessage, params string[] args)
            => Error(HttpStatusCode.InternalServerError, errorMessage, args);

        public static BaseResponse Forbid()
            => new BaseResponse((int)HttpStatusCode.Forbidden, default);

        public static BaseResponse Forbid(string errorMessage, params string[] args)
            => Error(HttpStatusCode.Forbidden, errorMessage, args);

        public static BaseResponse Error(HttpStatusCode code, string errorMessage, params string[] args)
            => new BaseResponse((int)code, new Dictionary<string, HttpResponseErrorModel[]>
            {
                { string.Empty, [new HttpResponseErrorModel(errorMessage, args)]}
            });

        public static BaseResponse Error(HttpStatusCode code, params IEnumerable<(string key, string errorMessage, string[] args)> errors)
            => new BaseResponse((int)code, errors
                .GroupBy(x => x.key)
                .ToDictionary(
                x => x.Key, 
                x => x.Select(x=> new HttpResponseErrorModel(x.errorMessage, x.args)).ToArray()));

        public static BaseResponse BadRequest(params IEnumerable<(string key, string errorMessage, string[] args)> errors)
            => Error(HttpStatusCode.BadRequest, errors);

        public new static BaseResponse StatusCode(HttpStatusCode code)
            => new BaseResponse((int)code, default);

        public static BaseResponse ModelState(ControllerBase controller, HttpStatusCode code)
            => new BaseResponse((int)code, ControllerResults.formatModelState(controller.ModelState));
    }
}
