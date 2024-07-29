using Microsoft.AspNetCore.Components;
using System.Text;

namespace NSL.Services.ServersHub.Client.Pages.Account.Pages
{
    public partial class ConfirmEmailChange
    {
        private string? message;

        //[CascadingParameter]
        //private HttpContext HttpContext { get; set; } = default!;

        [Parameter] public string? UserId { get; set; }

        [Parameter] public string? Email { get; set; }

        [Parameter] public string? Code { get; set; }

    //    protected override async Task OnInitializedAsync()
    //    {
    //        if (UserId is null || Email is null || Code is null)
    //        {
    //            RedirectManager.RedirectToWithStatus(
    //                "Account/Login", "Error: Invalid email change confirmation link.", HttpContext);
    //        }

    //        var user = await UserManager.FindByIdAsync(UserId);
    //        if (user is null)
    //        {
    //            message = "Unable to find user with Id '{userId}'";
    //            return;
    //        }

    //        var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(Code));
    //        var result = await UserManager.ChangeEmailAsync(user, Email, code);
    //        if (!result.Succeeded)
    //        {
    //            message = "Error changing email.";
    //            return;
    //        }

    //        // In our UI email and user name are one and the same, so when we update the email
    //        // we need to update the user name.
    //        var setUserNameResult = await UserManager.SetUserNameAsync(user, Email);
    //        if (!setUserNameResult.Succeeded)
    //        {
    //            message = "Error changing user name.";
    //            return;
    //        }

    //        await SignInManager.RefreshSignInAsync(user);
    //        message = "Thank you for confirming your email change.";
    //    }
    }
}