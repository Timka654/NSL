using Microsoft.AspNetCore.Components;
using NSL.BlazorTemplate.Shared.Client.DotNetIdentity;
using NSL.BlazorTemplate.Shared.Models;

namespace NSL.BlazorTemplate.Client.Pages.Account.Pages.Manage
{
    public partial class ExternalLogins
    {
        public const string LinkLoginCallbackAction = "LinkLoginCallback";

        private UserModel user = default!;
        private IList<UserLoginInfo>? currentLogins;
        private IList<AuthenticationScheme>? otherLogins;
        private bool showRemoveButton;

        //[CascadingParameter]
        //private HttpContext HttpContext { get; set; } = default!;

        private string? LoginProvider { get; set; }

        private string? ProviderKey { get; set; }

        [Parameter] public string? Action { get; set; }

        //protected override async Task OnInitializedAsync()
        //{
        //    user = await UserAccessor.GetRequiredUserAsync(HttpContext);
        //    currentLogins = await UserManager.GetLoginsAsync(user);
        //    otherLogins = (await SignInManager.GetExternalAuthenticationSchemesAsync())
        //        .Where(auth => currentLogins.All(ul => auth.Name != ul.LoginProvider))
        //        .ToList();

        //    string? passwordHash = null;
        //    if (UserStore is IUserPasswordStore<ApplicationUser> userPasswordStore)
        //    {
        //        passwordHash = await userPasswordStore.GetPasswordHashAsync(user, HttpContext.RequestAborted);
        //    }

        //    showRemoveButton = passwordHash is not null || currentLogins.Count > 1;

        //    if (HttpMethods.IsGet(HttpContext.Request.Method) && Action == LinkLoginCallbackAction)
        //    {
        //        await OnGetLinkLoginCallbackAsync();
        //    }
        //}

        private async Task OnSubmitAsync()
        {
            //var result = await UserManager.RemoveLoginAsync(user, LoginProvider!, ProviderKey!);
            //if (!result.Succeeded)
            //{
            //    RedirectManager.RedirectToCurrentPageWithStatus("Error: The external login was not removed.", HttpContext);
            //}

            //await SignInManager.RefreshSignInAsync(user);
            //RedirectManager.RedirectToCurrentPageWithStatus("The external login was removed.", HttpContext);
        }

        private async Task OnGetLinkLoginCallbackAsync()
        {
            //var userId = await UserManager.GetUserIdAsync(user);
            //var info = await SignInManager.GetExternalLoginInfoAsync(userId);
            //if (info is null)
            //{
            //    RedirectManager.RedirectToCurrentPageWithStatus("Error: Could not load external login info.", HttpContext);
            //}

            //var result = await UserManager.AddLoginAsync(user, info);
            //if (!result.Succeeded)
            //{
            //    RedirectManager.RedirectToCurrentPageWithStatus("Error: The external login was not added. External logins can only be associated with one account.", HttpContext);
            //}

            //// Clear the existing external cookie to ensure a clean login process
            //await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            //RedirectManager.RedirectToCurrentPageWithStatus("The external login was added.", HttpContext);
        }
    }
}