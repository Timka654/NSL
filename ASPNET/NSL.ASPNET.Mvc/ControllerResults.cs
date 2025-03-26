using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.IO;
using System.Linq;
using System.Net;

namespace NSL.ASPNET.Mvc
{
    public class ControllerResults
    {
        public static IActionResult Ok()
            => new OkResult();

        public static IActionResult Ok(object value)
            => new OkObjectResult(value);

        public static IActionResult Ok<TResult>(TResult value)
            => new OkObjectResult(value);

        public static IActionResult NotFound()
            => new NotFoundResult();

        public static IActionResult NotFound(object value)
            => new NotFoundObjectResult(value);

        public static IActionResult NotFound<TResult>(TResult value)
            => new NotFoundObjectResult(value);

        public static IActionResult BadRequest()
            => new BadRequestResult();

        public static IActionResult BadRequest(object value)
            => new BadRequestObjectResult(value);

        public static IActionResult BadRequest<TResult>(TResult value)
            => new BadRequestObjectResult(value);

        public static IActionResult Unauthorized()
            => new UnauthorizedResult();

        public static IActionResult Unauthorized(object value)
            => new UnauthorizedObjectResult(value);

        public static IActionResult Unauthorized<TResult>(TResult value)
            => new UnauthorizedObjectResult(value);

        public static IActionResult StatusCode(HttpStatusCode code)
            => new StatusCodeResult((int)code);

        public static IActionResult FileSteam(FileStream fs, string contentType, string fileName, bool rangeProcessing = false, DateTimeOffset? lastModified = null)
            => new FileStreamResult(fs, contentType)
            {
                FileDownloadName = fileName,
                LastModified = lastModified,
                EnableRangeProcessing = rangeProcessing
            };

        public static IActionResult FileContent(byte[] content, string contentType, string fileName, bool rangeProcessing = false, DateTimeOffset? lastModified = null)
            => new FileContentResult(content, contentType)
            {
                FileDownloadName = fileName,
                LastModified = lastModified,
                EnableRangeProcessing = rangeProcessing
            };

        public static IActionResult PhysicalFile(string path, string contentType, string fileName = null, bool rangeProcessing = false, DateTimeOffset? lastModified = null)
            => new PhysicalFileResult(path, contentType)
            {
                FileName = fileName ?? Path.GetFileName(path),
                LastModified = lastModified,
                EnableRangeProcessing = rangeProcessing
            };

        public static IActionResult VirtualFile(string path, string contentType, string fileName = null, bool rangeProcessing = false, DateTimeOffset? lastModified = null)
            => new VirtualFileResult(path, contentType)
            {
                FileName = fileName ?? Path.GetFileName(path),
                LastModified = lastModified,
                EnableRangeProcessing = rangeProcessing
            };

        public static IActionResult LocalRedirect(string localUrl)
            => new LocalRedirectResult(localUrl);

        public static IActionResult LocalRedirect(string localUrl, bool permanent)
            => new LocalRedirectResult(localUrl, permanent);

        public static IActionResult LocalRedirect(string localUrl, bool permanent, bool preserveMethod)
            => new LocalRedirectResult(localUrl, permanent, preserveMethod);

        public static IActionResult Redirect(string localUrl)
            => new RedirectResult(localUrl);

        public static IActionResult Redirect(string localUrl, bool permanent)
            => new RedirectResult(localUrl, permanent);

        public static IActionResult Redirect(string localUrl, bool permanent, bool preserveMethod)
            => new RedirectResult(localUrl, permanent, preserveMethod);


        public static IActionResult IdResponse(object id)
            => Ok(new { id });

        public static IActionResult DataResponse(object data)
            => Ok(new { Data = data });

        public static IActionResult NotFoundResponse(ModelStateDictionary modelState, string errorMessage)
        {
            modelState.AddModelError(string.Empty, errorMessage);

            return NotFound(formatModelState(modelState));
        }

        public static IActionResult NotFoundResponse(ModelStateDictionary modelState)
            => NotFoundResponse(modelState, "{...no_found}");

        public static IActionResult ModelStateResponse(ModelStateDictionary modelState, string errorMessage)
        {
            modelState.AddModelError(string.Empty, errorMessage);

            return ModelStateResponse(modelState);
        }

        public static IActionResult ModelStateResponse(ModelStateDictionary modelState, string[] errorMessages)
        {
            foreach (var errorMessage in errorMessages)
            {
                modelState.AddModelError(string.Empty, errorMessage);
            }

            return ModelStateResponse(modelState);
        }

        public static IActionResult ModelStateResponse(ModelStateDictionary modelState, string errorKey, string errorMessage)
        {
            modelState.AddModelError(errorKey, errorMessage);

            return ModelStateResponse(modelState);
        }

        public static IActionResult ModelStateResponse(ModelStateDictionary modelState, params (string key, string message)[] errorMessages)
        {
            foreach (var errorMessage in errorMessages)
            {
                modelState.AddModelError(errorMessage.key, errorMessage.message);
            }

            return ModelStateResponse(modelState);
        }

        public static IActionResult ModelStateResponse(ModelStateDictionary modelState)
            => BadRequest(formatModelState(modelState));

        internal static object formatModelState(ModelStateDictionary modelState)
            => modelState.ToDictionary(x => x.Key, x => x.Value.Errors.Select(b => b.ErrorMessage).ToArray());
    }
}
