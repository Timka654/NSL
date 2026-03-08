using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSL.HttpClient;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace NSL.ASPNET.Routing.ErrorHandler
{
    public class NSLErrorsHandleService(ILogger<NSLErrorsHandleService> logger)
    {
        public virtual async Task<bool> ProcessError(Exception exception, HttpContext context)
        {
            var ex = exception;

            ex = PreProcessException(ex);

            if (ex is NSLBadRequestException badreqex)
            {
                context.Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsJsonAsync(new HttpErrorResponseData()
                    {
                        { badreqex.Key, badreqex.Messages }
                    });

                return true;
            }

            return false;
        }

        protected Exception PreProcessException(Exception ex)
        {
            if (ex is InvalidOperationException ioex)
            {
                ex = ex.InnerException ?? ex;
            }

            return ex;
        }

        protected async Task<string> ReadStringBody(HttpRequest request)
            =>Encoding.UTF8.GetString(await ReadByteBody(request));

        protected async Task<byte[]> ReadByteBody(HttpRequest request)
        {
            request.Body.Position = 0;  //rewinding the stream to 0

            var buffer = new byte[Convert.ToInt32(request.ContentLength)];

            await request.Body.ReadAsync(buffer);

            return buffer;
        }

        public virtual async Task ReportRequestError(HttpContext context, Exception ex)
        {
            var request = context.Request;

            var sb = new StringBuilder();

            sb.AppendLine($"Url: {context.Request.Path},");
            sb.AppendLine($"User: {(context.User?.GetUserGuidId()?.ToString() ?? "none")},");

            if (request.Method == HttpMethods.Post && request.ContentLength > 0)
            {
                sb.AppendLine($"Body: {await ReadStringBody(request)},");
            }

            sb.AppendLine($"Exception: {ex.ToString()}");

            logger.LogError(sb.ToString());

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(
                "{ \"\": [ \"{internal_server_error_message}\"] }");
        }
    }
}
