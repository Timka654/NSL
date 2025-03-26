using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using NSL.BlazorTemplate.Client.Services;
using NSL.BlazorTemplate.Shared.Client.DotNetIdentity;
using NSL.BlazorTemplate.Shared.Models.RequestModels;
using System.Text;
using System.Text.Encodings.Web;

namespace NSL.BlazorTemplate.Client.Pages.Account.Pages
{
    public partial class Register
    {
        private IEnumerable<IdentityError>? identityErrors;

        private IdentityRegisterRequestModel Input { get; set; } = new();

        [SupplyParameterFromQuery] public string? ReturnUrl { get; set; }

        private string? Message => identityErrors is null ? null : $"Error: {string.Join(", ", identityErrors.Select(error => error.Description))}";

        [Inject] NavigationManager NavigationManager { get; set; }

        [Inject] HubIdentityService HubIdentityService { get; set; }

        public async Task RegisterUser(EditContext editContext)
        {
            var response = await HubIdentityService.IdentityRegisterPostRequest(Input);

            if (response.IsSuccess)
            {
                await HubIdentityService.SetIdentityAsync(response.Data);
                NavigationManager.NavigateTo(ReturnUrl ?? string.Empty);
            }
            else if (response.IsBadRequest)
                identityErrors = response.Errors.SelectMany(x => x.Value).Select(x => new IdentityError() { Description = x }).ToArray();


            //var user = CreateUser();

            //await UserStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
            //var emailStore = GetEmailStore();
            //await emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
            //var result = await UserManager.CreateAsync(user, Input.Password);

            //if (!result.Succeeded)
            //{
            //    identityErrors = result.Errors;
            //    return;
            //}

            //Logger.LogInformation("User created a new account with password.");

            //var userId = await UserManager.GetUserIdAsync(user);
            //var code = await UserManager.GenerateEmailConfirmationTokenAsync(user);
            //code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            //var callbackUrl = NavigationManager.GetUriWithQueryParameters(
            //    NavigationManager.ToAbsoluteUri("Account/ConfirmEmail").AbsoluteUri,
            //    new Dictionary<string, object?> { ["userId"] = userId, ["code"] = code, ["returnUrl"] = ReturnUrl });

            //await EmailSender.SendConfirmationLinkAsync(user, Input.Email, HtmlEncoder.Default.Encode(callbackUrl));

            //if (UserManager.Options.SignIn.RequireConfirmedAccount)
            //{
            //    RedirectManager.RedirectTo(
            //        "Account/RegisterConfirmation",
            //        new() { ["email"] = Input.Email, ["returnUrl"] = ReturnUrl });
            //}

            //await SignInManager.SignInAsync(user, isPersistent: false);
            //RedirectManager.RedirectTo(ReturnUrl);
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
        //            $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor.");
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