using Microsoft.AspNetCore.Components;
using NSL.Services.ServersHub.Shared.Models;
using NSL.Services.ServersHub.Shared.Models.RequestModels;
using System.Text;
using System.Text.Encodings.Web;

namespace NSL.Services.ServersHub.Client.Pages.Account.Pages.Manage
{
    public partial class Email
    {
        private string? message;
        private UserModel user = default!;
        private string? email;
        private bool isEmailConfirmed;

        //[CascadingParameter]
        //private HttpContext HttpContext { get; set; } = default!;

        private IdentityEmailRequestModel Input { get; set; } = new();

        //protected override async Task OnInitializedAsync()
        //{
        //    user = await UserAccessor.GetRequiredUserAsync(HttpContext);
        //    email = await UserManager.GetEmailAsync(user);
        //    isEmailConfirmed = await UserManager.IsEmailConfirmedAsync(user);

        //    Input.NewEmail ??= email;
        //}

        private async Task OnValidSubmitAsync()
        {
            //if (Input.NewEmail is null || Input.NewEmail == email)
            //{
            //    message = "Your email is unchanged.";
            //    return;
            //}

            //var userId = await UserManager.GetUserIdAsync(user);
            //var code = await UserManager.GenerateChangeEmailTokenAsync(user, Input.NewEmail);
            //code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            //var callbackUrl = NavigationManager.GetUriWithQueryParameters(
            //    NavigationManager.ToAbsoluteUri("Account/ConfirmEmailChange").AbsoluteUri,
            //    new Dictionary<string, object?> { ["userId"] = userId, ["email"] = Input.NewEmail, ["code"] = code });

            //await EmailSender.SendConfirmationLinkAsync(user, Input.NewEmail, HtmlEncoder.Default.Encode(callbackUrl));

            //message = "Confirmation link to change email sent. Please check your email.";
        }

        private async Task OnSendEmailVerificationAsync()
        {
            //if (email is null)
            //{
            //    return;
            //}

            //var userId = await UserManager.GetUserIdAsync(user);
            //var code = await UserManager.GenerateEmailConfirmationTokenAsync(user);
            //code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            //var callbackUrl = NavigationManager.GetUriWithQueryParameters(
            //    NavigationManager.ToAbsoluteUri("Account/ConfirmEmail").AbsoluteUri,
            //    new Dictionary<string, object?> { ["userId"] = userId, ["code"] = code });

            //await EmailSender.SendConfirmationLinkAsync(user, email, HtmlEncoder.Default.Encode(callbackUrl));

            //message = "Verification email sent. Please check your email.";
        }
    }
}