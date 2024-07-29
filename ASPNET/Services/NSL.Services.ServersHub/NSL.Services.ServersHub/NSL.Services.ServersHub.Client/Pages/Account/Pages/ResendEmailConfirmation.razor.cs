using Microsoft.AspNetCore.Components;
using NSL.Services.ServersHub.Shared.Models.RequestModels;
using System.Text;
using System.Text.Encodings.Web;

namespace NSL.Services.ServersHub.Client.Pages.Account.Pages
{
    public partial class ResendEmailConfirmation
    {
        private string? message;

        private IdentityResendEmailConfirmationRequestModel Input { get; set; } = new();

        private async Task OnValidSubmitAsync()
        {
            //var user = await UserManager.FindByEmailAsync(Input.Email!);
            //if (user is null)
            //{
            //    message = "Verification email sent. Please check your email.";
            //    return;
            //}

            //var userId = await UserManager.GetUserIdAsync(user);
            //var code = await UserManager.GenerateEmailConfirmationTokenAsync(user);
            //code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            //var callbackUrl = NavigationManager.GetUriWithQueryParameters(
            //    NavigationManager.ToAbsoluteUri("Account/ConfirmEmail").AbsoluteUri,
            //    new Dictionary<string, object?> { ["userId"] = userId, ["code"] = code });
            //await EmailSender.SendConfirmationLinkAsync(user, Input.Email, HtmlEncoder.Default.Encode(callbackUrl));

            //message = "Verification email sent. Please check your email.";
        }
    }
}