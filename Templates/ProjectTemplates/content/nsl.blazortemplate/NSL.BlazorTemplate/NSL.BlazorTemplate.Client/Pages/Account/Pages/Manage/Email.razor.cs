using Microsoft.AspNetCore.Components;
using NSL.BlazorTemplate.Client.Services;
using NSL.BlazorTemplate.Shared.Models;
using NSL.BlazorTemplate.Shared.Models.RequestModels;

namespace NSL.BlazorTemplate.Client.Pages.Account.Pages.Manage
{
    public partial class Email
    {
        private string? message;
        private UserModel user = default!;

        private IdentityEmailRequestModel Input { get; set; } = new();

        [Inject] HubIdentityService IdentityService { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            var response = await IdentityService.UserDetailsPostRequest();

            if (!response.IsSuccess)
                return;

            user = response.Data;
        }

        private async Task OnValidSubmitAsync()
        {
            if (Input.NewEmail is null || Input.NewEmail == user.Email)
            {
                message = "Your email is unchanged.";
                return;
            }

            var response = await IdentityService.UserChangeEmailPostRequest(Input);

            if (!response.IsSuccess)
                return;

            user.Email = Input.NewEmail;

            Input = new IdentityEmailRequestModel();

            message = "Confirmation link to change email sent. Please check your email.";

            StateHasChanged();
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