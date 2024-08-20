using Microsoft.AspNetCore.Components;
using NSL.BlazorTemplate.Shared.Models;
using NSL.BlazorTemplate.Shared.Models.RequestModels;

namespace NSL.BlazorTemplate.Client.Pages.Account.Pages
{
    public partial class LoginWithRecoveryCode
    {
        private string? message;
        private UserModel user = default!;

        private IdentityLoginWithRecoveryCodeRequestModel Input { get; set; } = new();

        [SupplyParameterFromQuery] public string? ReturnUrl { get; set; }

        //protected override async Task OnInitializedAsync()
        //{
        //    // Ensure the user has gone through the username & password screen first
        //    user = await SignInManager.GetTwoFactorAuthenticationUserAsync() ??
        //        throw new InvalidOperationException("Unable to load two-factor authentication user.");
        //}

        private async Task OnValidSubmitAsync()
        {
            //var recoveryCode = Input.RecoveryCode.Replace(" ", string.Empty);

            //var result = await SignInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

            //var userId = await UserManager.GetUserIdAsync(user);

            //if (result.Succeeded)
            //{
            //    Logger.LogInformation("User with ID '{UserId}' logged in with a recovery code.", userId);
            //    RedirectManager.RedirectTo(ReturnUrl);
            //}
            //else if (result.IsLockedOut)
            //{
            //    Logger.LogWarning("User account locked out.");
            //    RedirectManager.RedirectTo("Account/Lockout");
            //}
            //else
            //{
            //    Logger.LogWarning("Invalid recovery code entered for user with ID '{UserId}' ", userId);
            //    message = "Error: Invalid recovery code entered.";
            //}
        }
    }
}