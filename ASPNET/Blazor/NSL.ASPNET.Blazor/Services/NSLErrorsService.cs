using Microsoft.AspNetCore.Components.Web;
using System;

namespace NSL.ASPNET.Blazor.Services
{

    public delegate void ErrorEventHandler(Exception ex, ErrorBoundary errorBoundary);

    public class NSLErrorsService
    {
        public event ErrorEventHandler OnError = (ex, errorBoundary) => { };
        public void CatchError(Exception ex, ErrorBoundary errorBoundary)
        {
            OnError(ex, errorBoundary);
        }
    }
}
