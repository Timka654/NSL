using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using NSL.ASPNET.Blazor.Services;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NSL.ASPNET.Blazor.Components.ErrorHandle
{
    public abstract partial class ErrorHandleComponent : ComponentBase, IDisposable
    {
        [Inject] NavigationManager NavigationManager { get; set; }

        [Inject] NSLErrorsService ErrorsService { get; set; }

        public virtual void Dispose()
        {
            ErrorsService.OnError -= TeachingReportErrorModalComponent_onExceptionCatch;
        }

        protected override void OnInitialized()
        {
            ErrorsService.OnError += TeachingReportErrorModalComponent_onExceptionCatch;
        }

        protected ErrorHandleContext? CurrentContext { get; set; } = new ErrorHandleContext();

        protected bool Reported { get; set; } = false;

        protected virtual Task<string> GenerateHash(Exception obj, ErrorBoundary errorBoundary)
            => Task.FromResult(string.Join('-', SHA1.HashData(Encoding.UTF8.GetBytes(obj.ToString() + NavigationManager.Uri)).Select(x => x.ToString("x2"))));

        protected abstract Task<bool> SkipError(Exception obj, string hash);

        protected virtual async void TeachingReportErrorModalComponent_onExceptionCatch(Exception obj, ErrorBoundary errorBoundary)
        => await ExceptionHandle(obj, errorBoundary);


        protected virtual async Task<bool> ExceptionHandle(Exception obj, ErrorBoundary errorBoundary)
        {
            var hash = await GenerateHash(obj, errorBoundary);

            Reported = await SkipError(obj, hash);

            CurrentContext = new ErrorHandleContext()
            {
                Exception = obj,
                Url = NavigationManager.Uri,
                Hash = hash,
                ErrorBoundary = errorBoundary
            };

            return true;
        }


        protected virtual async Task ToHome()
        {
            NavigationManager.NavigateTo("/", true);
        }

        protected async Task ReloadPage()
        {
            CurrentContext.ErrorBoundary?.Recover();
        }
    }
}
