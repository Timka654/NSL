namespace NSL.BlazorTemplate.Client.Pages.Account.Pages.Manage
{
    public partial class ResetAuthenticator
    {
        //[CascadingParameter]
        //private HttpContext HttpContext { get; set; } = default!;

        private async Task OnSubmitAsync()
        {
            //var user = await UserAccessor.GetRequiredUserAsync(HttpContext);
            //await UserManager.SetTwoFactorEnabledAsync(user, false);
            //await UserManager.ResetAuthenticatorKeyAsync(user);
            //var userId = await UserManager.GetUserIdAsync(user);
            //Logger.LogInformation("User with ID '{UserId}' has reset their authentication app key.", userId);

            //await SignInManager.RefreshSignInAsync(user);

            //RedirectManager.RedirectToWithStatus(
            //    "Account/Manage/EnableAuthenticator",
            //    "Your authenticator app key has been reset, you will need to configure your authenticator app using the new key.",
            //    HttpContext);
        }
    }
}