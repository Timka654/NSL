using System.Net;

namespace DevExtensions.Blazor.Http.Models
{
    public class BaseHttpResponse : IDisposable
    {
        public HttpStatusCode StatusCode { get; set; }

        public bool IsSuccess => StatusCode == HttpStatusCode.OK;

        public bool IsBadRequest => StatusCode == HttpStatusCode.BadRequest;

        public HttpResponseMessage Response { get; set; }

        public string? Content { get; set; }

        /// <summary>
        /// Dispose <see cref="Response"/> prop
        /// </summary>
        public void Dispose()
        {
            Response?.Dispose();
        }
    }
}
