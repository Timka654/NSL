using Microsoft.AspNetCore.Components;
using NSL.Services.ServersHub.Shared.Models;
using NSL.Services.ServersHub.Shared.Models.RequestModels;

namespace NSL.Services.ServersHub.Client.Pages.Account.Pages.Manage
{
    public partial class SetPassword
    {
        private string? message;
        private UserModel user = default!;

        //[CascadingParameter]
        //private HttpContext HttpContext { get; set; } = default!;

        private IdentitySetPasswordRequestModel Input { get; set; } = new();

        //protected override async Task OnInitializedAsync()
        //{
        //    user = await UserAccessor.GetRequiredUserAsync(HttpContext);

        //    var hasPassword = await UserManager.HasPasswordAsync(user);
        //    if (hasPassword)
        //    {
        //        RedirectManager.RedirectTo("Account/Manage/ChangePassword");
        //    }
        //}

        private async Task OnValidSubmitAsync()
        {
            //var addPasswordResult = await UserManager.AddPasswordAsync(user, Input.NewPassword!);
            //if (!addPasswordResult.Succeeded)
            //{
            //    message = $"Error: {string.Join(",", addPasswordResult.Errors.Select(error => error.Description))}";
            //    return;
            //}

            //await SignInManager.RefreshSignInAsync(user);
            //RedirectManager.RedirectToCurrentPageWithStatus("Your password has been set.", HttpContext);
        }
    }
}