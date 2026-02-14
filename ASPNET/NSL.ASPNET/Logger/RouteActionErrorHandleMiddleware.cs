using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NSL.ASPNET.Logger
{
    public class RouteActionErrorHandleMiddleware(ILogger<RouteActionErrorHandleMiddleware> logger) : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var request = context.Request;
            try
            {
                if (request.HasFormContentType || request.HasJsonContentType())
                    request.EnableBuffering();

                await next(context);
            }
            catch (OperationCanceledException) { }
            catch (Exception _ex)
            {
                var ex = _ex;

                if (ex is InvalidOperationException ioex)
                {
                    if (ex.InnerException != null)
                    {
                        ex = ex.InnerException;
                    }
                }


                if (ex is OperationCanceledException) return;
                if (ex is IOException) return;

                await ReportErrors(context, _ex);
                //throw;
            }
        }

        protected async Task ReportErrors(HttpContext context, Exception ex)
        {
            var request = context.Request;
            var sb = new StringBuilder();

            sb.AppendLine($"Url: {request.Path}");
            sb.AppendLine($"Method: {request.Method}");
            sb.AppendLine($"User: {context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value}");

            try
            {
                // Проверяем, есть ли тело и можно ли его прочитать несколько раз
                if (request.ContentLength > 0)
                {
                    // Важно: EnableBuffering() должен быть вызван ранее в цепочке Middleware
                    if (request.Body.CanSeek)
                    {
                        request.Body.Position = 0;

                        if (request.HasFormContentType)
                        {
                            // Обработка форм и файлов
                            var form = await request.ReadFormAsync();
                            sb.AppendLine("Body (Form Data):");

                            // Текстовые поля
                            foreach (var key in form.Keys)
                            {
                                sb.AppendLine($"  {key}: {form[key]}");
                            }

                            // Метаданные файлов
                            if (form.Files.Any())
                            {
                                sb.AppendLine("Files:");
                                foreach (var file in form.Files)
                                    sb.AppendLine($"  - {file.Name}: {file.FileName} ({file.Length} bytes, {file.ContentType})");
                            }
                        }
                        else if (request.HasJsonContentType())
                        {
                            request.Body.Position = 0;  //rewinding the stream to 0

                            using (var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true))
                            {
                                var requestContent = await reader.ReadToEndAsync();
                                sb.AppendLine($"Body (JSON): {requestContent}");
                            }
                        }
                        else
                            sb.AppendLine($"Body ({request.ContentType}): <unhandled content type>,");
                    }
                    else
                        sb.AppendLine($"Body: <cannot seek>,");
                }
                else
                    sb.AppendLine($"Body: <empty>,");

            }
            catch (Exception readEx)
            {
                sb.AppendLine($"Could not read body: {readEx}");
            }

            sb.AppendLine($"Exception: {ex}");
            logger.LogError(sb.ToString());

            // Ответ клиенту
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{ \"error\": [ \"internal_server_error_message\" ] }");
        }
    }

    public static class RouteActionErrorHandleMiddlewareExtensions
    {
        public static IServiceCollection AddRouteActionErrorHandleMiddleware(this IServiceCollection services)
        {
            services.AddSingleton<RouteActionErrorHandleMiddleware>();

            return services;
        }

        public static IApplicationBuilder UseRouteActionErrorHandleMiddleware(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<RouteActionErrorHandleMiddleware>();

            return builder;
        }
    }
}
