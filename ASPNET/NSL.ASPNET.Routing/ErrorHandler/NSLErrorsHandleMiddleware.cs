using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace NSL.ASPNET.Routing.ErrorHandler
{
    public class NSLErrorsHandleMiddleware(NSLErrorsHandleService errorsHandleService) : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var request = context.Request;
            try
            {
                if (request.Method == HttpMethods.Post && request.ContentLength > 0)
                    request.EnableBuffering();

                await next(context);
            }
            catch (OperationCanceledException) { }
            catch (Exception _ex)
            {
                var ex = _ex;

                if (await errorsHandleService.ProcessError(ex, context))
                    return;

                if (ex is OperationCanceledException) return;
                if (ex is IOException) return;

                await errorsHandleService.ReportRequestError(context, _ex);
                //throw;
            }
        }


    }
}
