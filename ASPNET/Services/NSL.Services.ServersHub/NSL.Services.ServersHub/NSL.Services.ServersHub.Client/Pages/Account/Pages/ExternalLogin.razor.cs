using Microsoft.AspNetCore.Components;
using NSL.Services.ServersHub.Shared.Models.RequestModels;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace NSL.Services.ServersHub.Client.Pages.Account.Pages
{
    public partial class ExternalLogin
    {
        public const string LoginCallbackAction = "LoginCallback";

        private string? message;
        //private ExternalLoginInfo externalLoginInfo = default!;

        //[CascadingParameter]
        //private HttpContext HttpContext { get; set; } = default!;

        private IdentityExternalLoginRequestModel Input { get; set; } = new();

        [Parameter] public string? RemoteError { get; set; }

        [SupplyParameterFromQuery] public string? ReturnUrl { get; set; }

        [Parameter] public string? Action { get; set; }

        private string? ProviderDisplayName => string.Empty;// externalLoginInfo.ProviderDisplayName;

        //protected override async Task OnInitializedAsync()
        //{
        //    if (RemoteError is not null)
        //    {
        //        RedirectManager.RedirectToWithStatus("Account/Login", $"Error from external provider: {RemoteError}", HttpContext);
        //    }

        //    var info = await SignInManager.GetExternalLoginInfoAsync();
        //    if (info is null)
        //    {
        //        RedirectManager.RedirectToWithStatus("Account/Login", "Error loading external login information.", HttpContext);
        //    }

        //    externalLoginInfo = info;

        //    if (HttpMethods.IsGet(HttpContext.Request.Method))
        //    {
        //        if (Action == LoginCallbackAction)
        //        {
        //            await OnLoginCallbackAsync();
        //            return;
        //        }

        //        // We should only reach this page via the login callback, so redirect back to
        //        // the login page if we get here some other way.
        //        RedirectManager.RedirectTo("Account/Login");
        //    }
        //}

        private async Task OnLoginCallbackAsync()
        {
            //// Sign in the user with this external login provider if the user already has a login.
            //var result = await SignInManager.ExternalLoginSignInAsync(
            //    externalLoginInfo.LoginProvider,
            //    externalLoginInfo.ProviderKey,
            //    isPersistent: false,
            //    bypassTwoFactor: true);

            //if (result.Succeeded)
            //{
            //    Logger.LogInformation(
            //        "{Name} logged in with {LoginProvider} provider.",
            //        externalLoginInfo.Principal.Identity?.Name,
            //        externalLoginInfo.LoginProvider);
            //    RedirectManager.RedirectTo(ReturnUrl);
            //}
            //else if (result.IsLockedOut)
            //{
            //    RedirectManager.RedirectTo("Account/Lockout");
            //}

            //// If the user does not have an account, then ask the user to create an account.
            //if (externalLoginInfo.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
            //{
            //    Input.Email = externalLoginInfo.Principal.FindFirstValue(ClaimTypes.Email) ?? "";
            //}
        }

        private async Task OnValidSubmitAsync()
        {
            //var emailStore = GetEmailStore();
            //var user = CreateUser();

            //await UserStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
            //await emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

            //var result = await UserManager.CreateAsync(user);
            //if (result.Succeeded)
            //{
            //    result = await UserManager.AddLoginAsync(user, externalLoginInfo);
            //    if (result.Succeeded)
            //    {
            //        Logger.LogInformation("User created an account using {Name} provider.", externalLoginInfo.LoginProvider);

            //        var userId = await UserManager.GetUserIdAsync(user);
            //        var code = await UserManager.GenerateEmailConfirmationTokenAsync(user);
            //        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            //        var callbackUrl = NavigationManager.GetUriWithQueryParameters(
            //            NavigationManager.ToAbsoluteUri("Account/ConfirmEmail").AbsoluteUri,
            //            new Dictionary<string, object?> { ["userId"] = userId, ["code"] = code });
            //        await EmailSender.SendConfirmationLinkAsync(user, Input.Email, HtmlEncoder.Default.Encode(callbackUrl));

            //        // If account confirmation is required, we need to show the link if we don't have a real email sender
            //        if (UserManager.Options.SignIn.RequireConfirmedAccount)
            //        {
            //            RedirectManager.RedirectTo("Account/RegisterConfirmation", new() { ["email"] = Input.Email });
            //        }

            //        await SignInManager.SignInAsync(user, isPersistent: false, externalLoginInfo.LoginProvider);
            //        RedirectManager.RedirectTo(ReturnUrl);
            //    }
            //}

            //message = $"Error: {string.Join(",", result.Errors.Select(error => error.Description))}";
        }

        //private ApplicationUser CreateUser()
        //{
        //    try
        //    {
        //        return Activator.CreateInstance<ApplicationUser>();
        //    }
        //    catch
        //    {
        //        throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
        //            $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor");
        //    }
        //}

        //private IUserEmailStore<ApplicationUser> GetEmailStore()
        //{
        //    if (!UserManager.SupportsUserEmail)
        //    {
        //        throw new NotSupportedException("The default UI requires a user store with email support.");
        //    }
        //    return (IUserEmailStore<ApplicationUser>)UserStore;
        //}

    }
}