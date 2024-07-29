using Microsoft.AspNetCore.Components;
using NSL.Services.ServersHub.Shared.Models;
using NSL.Services.ServersHub.Shared.Models.RequestModels;

namespace NSL.Services.ServersHub.Client.Pages.Account.Pages.Manage
{
    public partial class Index
    {
        private UserModel user = default!;
        private string? username;
        private string? phoneNumber;

        //[CascadingParameter]
        //private HttpContext HttpContext { get; set; } = default!;

        private IdentityIndexRequestModel Input { get; set; } = new();

        //protected override async Task OnInitializedAsync()
        //{
        //    user = await UserAccessor.GetRequiredUserAsync(HttpContext);
        //    username = await UserManager.GetUserNameAsync(user);
        //    phoneNumber = await UserManager.GetPhoneNumberAsync(user);

        //    Input.PhoneNumber ??= phoneNumber;
        //}

        private async Task OnValidSubmitAsync()
        {
            //if (Input.PhoneNumber != phoneNumber)
            //{
            //    var setPhoneResult = await UserManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
            //    if (!setPhoneResult.Succeeded)
            //    {
            //        RedirectManager.RedirectToCurrentPageWithStatus("Error: Failed to set phone number.", HttpContext);
            //    }
            //}

            //await SignInManager.RefreshSignInAsync(user);
            //RedirectManager.RedirectToCurrentPageWithStatus("Your profile has been updated", HttpContext);
        }
    }
}