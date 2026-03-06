using Microsoft.AspNetCore.Components;
using NSL.BlazorTemplate.Shared.Client.DotNetIdentity;
using NSL.BlazorTemplate.Shared.Models.RequestModels;
using System.Text;

namespace NSL.BlazorTemplate.Client.Pages.Account.Pages
{
    public partial class ResetPassword
    {
        private IEnumerable<IdentityError>? identityErrors;

        private IdentityResetPasswordRequestModel Input { get; set; } = new();

        [Parameter] public string? Code { get; set; }

        private string? Message => identityErrors is null ? null : $"Error: {string.Join(", ", identityErrors.Select(error => error.Description))}";

        //protected override void OnInitialized()
        //{
        //    if (Code is null)
        //    {
        //        RedirectManager.RedirectTo("Account/InvalidPasswordReset");
        //    }

        //    Input.Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(Code));
        //}

        private async Task OnValidSubmitAsync()
        {
            //var user = await UserManager.FindByEmailAsync(Input.Email);
            //if (user is null)
            //{
            //    // Don't reveal that the user does not exist
            //    RedirectManager.RedirectTo("Account/ResetPasswordConfirmation");
            //}

            //var result = await UserManager.ResetPasswordAsync(user, Input.Code, Input.Password);
            //if (result.Succeeded)
            //{
            //    RedirectManager.RedirectTo("Account/ResetPasswordConfirmation");
            //}

            //identityErrors = result.Errors;
        }
    }
}