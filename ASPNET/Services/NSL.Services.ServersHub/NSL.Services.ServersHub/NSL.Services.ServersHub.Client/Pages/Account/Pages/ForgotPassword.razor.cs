using Microsoft.AspNetCore.Components;
using NSL.Services.ServersHub.Shared.Models.RequestModels;
using System.Text;
using System.Text.Encodings.Web;

namespace NSL.Services.ServersHub.Client.Pages.Account.Pages
{
    public partial class ForgotPassword
    {
        private IdentityForgotPasswordRequestModel Input { get; set; } = new();

        private async Task OnValidSubmitAsync()
        {
            //var user = await UserManager.FindByEmailAsync(Input.Email);
            //if (user is null || !(await UserManager.IsEmailConfirmedAsync(user)))
            //{
            //    // Don't reveal that the user does not exist or is not confirmed
            //    RedirectManager.RedirectTo("Account/ForgotPasswordConfirmation");
            //}

            //// For more information on how to enable account confirmation and password reset please
            //// visit https://go.microsoft.com/fwlink/?LinkID=532713
            //var code = await UserManager.GeneratePasswordResetTokenAsync(user);
            //code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            //var callbackUrl = NavigationManager.GetUriWithQueryParameters(
            //    NavigationManager.ToAbsoluteUri("Account/ResetPassword").AbsoluteUri,
            //    new Dictionary<string, object?> { ["code"] = code });

            //await EmailSender.SendPasswordResetLinkAsync(user, Input.Email, HtmlEncoder.Default.Encode(callbackUrl));

            //RedirectManager.RedirectTo("Account/ForgotPasswordConfirmation");
        }
    }
}