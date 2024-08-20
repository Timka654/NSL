using Microsoft.AspNetCore.Components;
using NSL.BlazorTemplate.Shared.Models;
using NSL.BlazorTemplate.Shared.Models.RequestModels;

namespace NSL.BlazorTemplate.Client.Pages.Account.Pages.Manage
{
    public partial class DeletePersonalData
    {
        private string? message;
        private UserModel user = default!;
        private bool requirePassword;

        //[CascadingParameter]
        //private HttpContext HttpContext { get; set; } = default!;

        private IdentityDeletePersonalDataRequestModel Input { get; set; } = new();

        //protected override async Task OnInitializedAsync()
        //{
        //    Input ??= new();
        //    user = await UserAccessor.GetRequiredUserAsync(HttpContext);
        //    requirePassword = await UserManager.HasPasswordAsync(user);
        //}

        private async Task OnValidSubmitAsync()
        {
            //if (requirePassword && !await UserManager.CheckPasswordAsync(user, Input.Password))
            //{
            //    message = "Error: Incorrect password.";
            //    return;
            //}

            //var result = await UserManager.DeleteAsync(user);
            //if (!result.Succeeded)
            //{
            //    throw new InvalidOperationException("Unexpected error occurred deleting user.");
            //}

            //await SignInManager.SignOutAsync();

            //var userId = await UserManager.GetUserIdAsync(user);
            //Logger.LogInformation("User with ID '{UserId}' deleted themselves.", userId);

            //RedirectManager.RedirectToCurrentPage();
        }
    }
}