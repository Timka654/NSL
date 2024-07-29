using Microsoft.AspNetCore.Components;
using NSL.Services.ServersHub.Shared.Models;
using NSL.Services.ServersHub.Shared.Models.RequestModels;

namespace NSL.Services.ServersHub.Client.Pages.Account.Pages
{
    public partial class LoginWith2fa
    {
        private string? message;
        private UserModel user = default!;

        private IdentityLoginWith2faRequestModel Input { get; set; } = new();

        [SupplyParameterFromQuery] public string? ReturnUrl { get; set; }

        [Parameter] public bool RememberMe { get; set; }

        //protected override async Task OnInitializedAsync()
        //{
        //    // Ensure the user has gone through the username & password screen first
        //    user = await SignInManager.GetTwoFactorAuthenticationUserAsync() ??
        //        throw new InvalidOperationException("Unable to load two-factor authentication user.");
        //}

        private async Task OnValidSubmitAsync()
        {
            //var authenticatorCode = Input.TwoFactorCode!.Replace(" ", string.Empty).Replace("-", string.Empty);
            //var result = await SignInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, RememberMe, Input.RememberMachine);
            //var userId = await UserManager.GetUserIdAsync(user);

            //if (result.Succeeded)
            //{
            //    Logger.LogInformation("User with ID '{UserId}' logged in with 2fa.", userId);
            //    RedirectManager.RedirectTo(ReturnUrl);
            //}
            //else if (result.IsLockedOut)
            //{
            //    Logger.LogWarning("User with ID '{UserId}' account locked out.", userId);
            //    RedirectManager.RedirectTo("Account/Lockout");
            //}
            //else
            //{
            //    Logger.LogWarning("Invalid authenticator code entered for user with ID '{UserId}'.", userId);
            //    message = "Error: Invalid authenticator code.";
            //}
        }
    }
}