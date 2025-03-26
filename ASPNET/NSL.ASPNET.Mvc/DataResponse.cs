using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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

        public static DataResponse<TData> NotFound<TData>()
            => NotFound<TData>("{...no_found}");

        public static DataResponse<TData> NotFound<TData>(string errorMessage)
            => Error<TData>(HttpStatusCode.NotFound, errorMessage);

        public static DataResponse<TData> InternalServerError<TData>()
            => StatusCode<TData>(HttpStatusCode.InternalServerError);

        public static DataResponse<TData> InternalServerError<TData>(string errorMessage)
            => Error<TData>(HttpStatusCode.InternalServerError, errorMessage);

        public static DataResponse<TData> Forbid<TData>()
            => new DataResponse<TData>((int)HttpStatusCode.Forbidden, default);

        public static DataResponse<TData> Forbid<TData>(string errorMessage)
            => Error<TData>(HttpStatusCode.NotFound, errorMessage);

        public static DataResponse<TData> Error<TData>(HttpStatusCode code, string errorMessage)
            => new DataResponse<TData>((int)code, new
            {
                Key = string.Empty,
                Value = (string[])[errorMessage]
            });

        public static DataResponse<TData> StatusCode<TData>(HttpStatusCode code)
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

        public DataResponse(TData data) : base(new { data })
        {
        }

        public DataResponse(int statusCode, object value) : base(new { data = value })
        {
            this.StatusCode = statusCode;
        }
    }
}
