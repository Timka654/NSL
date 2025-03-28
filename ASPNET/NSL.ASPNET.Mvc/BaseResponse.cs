using Microsoft.AspNetCore.Mvc;
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

        public BaseResponse() : base(null)
        {
        }

        public BaseResponse(int statusCode, object value) : base(value)
        {
            base.StatusCode = statusCode;
        }

        public static BaseResponse Ok()
            => new BaseResponse();

        public static BaseResponse NotFound()
            => NotFound("{...no_found}");

        public static BaseResponse NotFound(string errorMessage)
            => Error(HttpStatusCode.NotFound, errorMessage);

        public static BaseResponse InternalServerError()
            => StatusCode(HttpStatusCode.InternalServerError);

        public static BaseResponse InternalServerError(string errorMessage)
            => Error(HttpStatusCode.InternalServerError, errorMessage);

        public static BaseResponse Forbid()
            => new BaseResponse((int)HttpStatusCode.Forbidden, default);

        public static BaseResponse Forbid(string errorMessage)
            => Error(HttpStatusCode.NotFound, errorMessage);

        public static BaseResponse Error(HttpStatusCode code, string errorMessage)
            => new BaseResponse((int)code, new
            {
                Key = string.Empty,
                Value = (string[])[errorMessage]
            });

        public static BaseResponse StatusCode(HttpStatusCode code)
            => new BaseResponse((int)code, default);

        public static BaseResponse ModelState(ControllerBase controller, HttpStatusCode code)
            => new BaseResponse((int)code, ControllerResults.formatModelState(controller.ModelState));
    }
}
