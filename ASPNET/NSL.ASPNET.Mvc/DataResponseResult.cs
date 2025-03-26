using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace NSL.ASPNET.Mvc
{
    public abstract class DataResponseResult
    {
        public static DataResponseResult<TData> Ok<TData>(TData data)
            => new DataResponseResult<TData>(data);

        public static DataResponseResult<TData> NotFound<TData>()
            => NotFound<TData>("{...no_found}");

        public static DataResponseResult<TData> NotFound<TData>(string errorMessage)
            => Error<TData>(HttpStatusCode.NotFound, errorMessage);

        public static DataResponseResult<TData> InternalServerError<TData>()
            => StatusCode<TData>(HttpStatusCode.InternalServerError);

        public static DataResponseResult<TData> InternalServerError<TData>(string errorMessage)
            => Error<TData>(HttpStatusCode.InternalServerError, errorMessage);

        public static DataResponseResult<TData> Forbid<TData>()
            => new DataResponseResult<TData>((int)HttpStatusCode.Forbidden, default);

        public static DataResponseResult<TData> Forbid<TData>(string errorMessage)
            => Error<TData>(HttpStatusCode.NotFound, errorMessage);

        public static DataResponseResult<TData> Error<TData>(HttpStatusCode code, string errorMessage)
            => new DataResponseResult<TData>((int)code, new
            {
                Key = string.Empty,
                Value = (string[])[errorMessage]
            });

        public static DataResponseResult<TData> StatusCode<TData>(HttpStatusCode code)
            => new DataResponseResult<TData>((int)code, default);

        public static DataListResponseResult<TData> ModelState<TData>(ControllerBase controller, HttpStatusCode code)
            => new DataListResponseResult<TData>((int)code, ControllerResults.formatModelState(controller.ModelState));
    }

    public class DataResponseResult<TData> : ObjectResult
    {
        public override Task ExecuteResultAsync(ActionContext context)
        {
            if (StatusCode != 200 && context.ModelState.Count > 0)
                Value = ControllerResults.formatModelState(context.ModelState);

            return base.ExecuteResultAsync(context);
        }

        public DataResponseResult(TData data) : base(new { data })
        {
        }

        public DataResponseResult(int statusCode, object value) : base(new { data = value })
        {
            this.StatusCode = statusCode;
        }
    }
}
