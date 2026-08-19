using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace NSL.ASPNET.Mvc
{

    public class BaseResponse : ObjectResult
    {
        //public override Task ExecuteResultAsync(ActionContext context)
        //{
        //    if (base.StatusCode != 200 && context.ModelState.Count > 0)
        //        Value = ControllerResults.formatModelState(context.ModelState);

        //    return base.ExecuteResultAsync(context);
        //}

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
            => Error(HttpStatusCode.NotFound, string.Empty, errorMessage, args);

        public static BaseResponse InternalServerError()
            => StatusCode(HttpStatusCode.InternalServerError);

        public static BaseResponse InternalServerError(string errorMessage, params string[] args)
            => Error(HttpStatusCode.InternalServerError, string.Empty, errorMessage, args);

        public static BaseResponse Forbid()
            => new BaseResponse((int)HttpStatusCode.Forbidden, default);

        public static BaseResponse Forbid(string errorMessage, params string[] args)
            => Error(HttpStatusCode.Forbidden, string.Empty, errorMessage, args);



        public new static BaseResponse StatusCode(HttpStatusCode code)
            => new BaseResponse((int)code, default);

        public static BaseResponse BadRequest(params IEnumerable<(string errorMessage, string[] args)> errors)
            => BadRequest(string.Empty, errors);

        public static BaseResponse BadRequest(string key, string errorMessage, string[] args)
            => Error(HttpStatusCode.BadRequest, key, errorMessage, args);

        public static BaseResponse BadRequest(string key,params IEnumerable<(string errorMessage, string[] args)> errors)
            => Error(HttpStatusCode.BadRequest, key, errors);



        public static BaseResponse Error(HttpStatusCode code, string field, string errorMessage, params string[] args)
            => new BaseResponse((int)code, new Dictionary<string, HttpResponseErrorModel[]>
            {
                { field, [new HttpResponseErrorModel(errorMessage, args)]}
            });

        public static BaseResponse Error(HttpStatusCode code, string field, IEnumerable<(string errorMessage, string[] args)> errors)
            => new BaseResponse((int)code, errors?.Any() == true ? new Dictionary<string, HttpResponseErrorModel[]>
            {
                { field, errors.Select(x => new HttpResponseErrorModel(x.errorMessage, x.args)).ToArray()}
            } : null);

        public static BaseResponse Error(HttpStatusCode code, params IEnumerable<(string key, string errorMessage, string[] args)> errors)
            => new BaseResponse((int)code, errors?.Any() == true ? errors
                .GroupBy(x => x.key)
                .ToDictionary(
                x => x.Key,
                x => x.Select(x => new HttpResponseErrorModel(x.errorMessage, x.args)).ToArray()) : null);


        public static BaseResponse ModelState(ControllerBase controller, HttpStatusCode code)
            => ModelState(controller.ModelState, code);


        public static BaseResponse ModelState(ActionExecutedContext controller, HttpStatusCode code)
            => ModelState(controller.ModelState, code);


        public static BaseResponse ModelState(ModelStateDictionary modelState, HttpStatusCode code)
            => new BaseResponse((int)code, ControllerResults.FormatModelState(modelState));
    }
}
