using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using NSL.Services.ServersHub.Client.Services;
using NSL.Services.ServersHub.Shared.Client.DotNetIdentity;
using NSL.Services.ServersHub.Shared.Models.RequestModels;
using System.ComponentModel.DataAnnotations;

namespace NSL.Services.ServersHub.Client.Pages.Account.Pages
{
    public partial class Login
    {
        private string? errorMessage;

        [Inject] NavigationManager NavigationManager { get; set; }
        [Inject] HubIdentityService HubIdentityService { get; set; }

        //[CascadingParameter]
        //private HttpContext HttpContext { get; set; } = default!;

        private IdentityLoginRequestModel Input { get; set; } = new();

        [SupplyParameterFromQuery] public string? ReturnUrl { get; set; }

        protected override Task OnInitializedAsync()
        {
            if (HubIdentityService.IsAuthenticated)
            {
                NavigationManager.NavigateTo(ReturnUrl ?? string.Empty);
            }

            return Task.CompletedTask;
        }

        public async Task LoginUser(EditContext context)
        {
            var response = await HubIdentityService.IdentityLoginPostRequest(Input);

            if (response.IsSuccess)
            {
                await HubIdentityService.SetIdentity(response.Data);
                NavigationManager.NavigateTo(ReturnUrl ?? string.Empty);
            }
            else if (response.IsBadRequest)
                errorMessage = response.Errors.SelectMany(x => x.Value).FirstOrDefault() ?? "Login error....";

            //// This doesn't count login failures towards account lockout
            //// To enable password failures to trigger account lockout, set lockoutOnFailure: true
            //var result = await SignInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
            //if (result.Succeeded)
            //{
            //    Logger.LogInformation("User logged in.");
            //    RedirectManager.RedirectTo(ReturnUrl);
            //}
            //else if (result.RequiresTwoFactor)
            //{
            //    RedirectManager.RedirectTo(
            //        "Account/LoginWith2fa",
            //        new() { ["returnUrl"] = ReturnUrl, ["rememberMe"] = Input.RememberMe });
            //}
            //else if (result.IsLockedOut)
            //{
            //    Logger.LogWarning("User account locked out.");
            //    RedirectManager.RedirectTo("Account/Lockout");
            //}
            //else
            //{
            //    errorMessage = "Error: Invalid login attempt.";
            //}
        }
    }
}