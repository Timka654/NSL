using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using NSL.BlazorTemplate.Client.Services;
using NSL.BlazorTemplate.Shared.Models;
using NSL.BlazorTemplate.Shared.Models.RequestModels;

namespace NSL.BlazorTemplate.Client.Pages.Account.Pages.Manage
{
    public partial class ChangePassword
    {
        private string? message;

        [Inject] HubIdentityService IdentityService { get; set; } = default!;

        [Inject] NavigationManager NavigationManager { get; set; } = default!;

        private IdentityChangePasswordRequestModel Input { get; set; } = new();

        private async Task OnValidSubmitAsync()
        {
            var response = await IdentityService.UserChangePasswordPostRequest(Input);

            if (response.IsSuccess)
            {
                await IdentityService.ClearIdentityAsync();

                NavigationManager.Refresh();
            }

            //     var changePasswordResult = await UserManager.ChangePasswordAsync(user, Input.OldPassword, Input.NewPassword);
            //     if (!changePasswordResult.Succeeded)
            //     {
            //         message = $"Error: {string.Join(",", changePasswordResult.Errors.Select(error => error.Description))}";
            //         return;
            //     }

            //     await SignInManager.RefreshSignInAsync(user);
            //     Logger.LogInformation("User changed their password successfully.");

            //     RedirectManager.RedirectToCurrentPageWithStatus("Your password has been changed", HttpContext);
        }
    }
}