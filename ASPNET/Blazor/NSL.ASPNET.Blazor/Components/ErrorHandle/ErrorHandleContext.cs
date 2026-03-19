using Microsoft.AspNetCore.Components.Web;
using System;

namespace NSL.ASPNET.Blazor.Components.ErrorHandle
{
    public class ErrorHandleContext
    {
        public Exception Exception { get; set; }

        public string? Url { get; set; }

        public string? AdditionalInformation { get; set; }

        public string? Hash { get; set; }

        public ErrorBoundary ErrorBoundary { get; set; }
    }
}
