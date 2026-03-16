using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using NSL.ASPNET.Blazor.Services;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NSL.ASPNET.Blazor.Components
{
    public abstract partial class ErrorHandleComponent : ComponentBase, IDisposable
    {
        [Inject] NavigationManager NavigationManager { get; set; }

        [Inject] NSLErrorsService ErrorsService { get; set; }

        [Inject] IJSRuntime js { get; set; }

        public void Dispose()
        {
            ErrorsService.OnError -= TeachingReportErrorModalComponent_onExceptionCatch;
        }

        protected override void OnInitialized()
        {
            ErrorsService.OnError += TeachingReportErrorModalComponent_onExceptionCatch;
        }

        protected ErrorHandleContext? CurrentContext { get; set; }

        bool reported = false;

        protected virtual Task<string> GenerateHash(Exception obj, ErrorBoundary errorBoundary)
            => Task.FromResult(string.Join('-', SHA1.HashData(Encoding.UTF8.GetBytes(obj.ToString() + NavigationManager.Uri)).Select(x => x.ToString("x2"))));

        protected abstract Task<bool> SkipError(Exception obj, string hash);

        protected virtual async void TeachingReportErrorModalComponent_onExceptionCatch(Exception obj, ErrorBoundary errorBoundary)
        => await ExceptionHandle(obj, errorBoundary);


        protected virtual async Task<bool> ExceptionHandle(Exception obj, ErrorBoundary errorBoundary)
        {
            var hash = await GenerateHash(obj, errorBoundary);

            reported = !await SkipError(obj, hash);

            CurrentContext = reported ? null : new ErrorHandleContext()
            {
                Exception = obj,
                Url = NavigationManager.Uri,
                Hash = hash,
                ErrorBoundary = errorBoundary
            };

            return true;
        }

        protected virtual async Task HideWindow()
        {
            await ReloadPage();
            //await modalRef.HideAsync();
        }

        protected virtual async Task ToHome()
        {
            NavigationManager.NavigateTo("/", true);
        }

        protected async Task ReloadPage()
        {
            CurrentContext.ErrorBoundary?.Recover();
        }

        //=> PreloadService.ProcessRequest(async () =>
        //{
        //    var response = await IdentityService.SystemReportClientErrorPostRequest(new Shared.Models.RequestModels.ReportClientErrorRequestModel()
        //    {
        //        Error = currentException.ToString(),
        //        Url = currentUrl,
        //        AdditionalInformation = additionalInformation,
        //        CompanyBranchId = CompanyBranchService.CurrentBranchId,
        //        CompanyId = CompanyService.CurrentCompanyId
        //    });

        //    if (!response.IsSuccess) return;

        //    await LocalStorageService.SetItemAsStringAsync($"err_{currentHash}", string.Empty);
        //    await HideWindow();
        //});
    }

    public class ErrorHandleContext
    {
        public Exception Exception { get; set; }

        public string? Url { get; set; }

        public string? AdditionalInformation { get; set; }

        public string? Hash { get; set; }

        public ErrorBoundary ErrorBoundary { get; set; }
    }
}
