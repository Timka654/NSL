using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NSL.BlazorTemplate.Shared.Models;

namespace NSL.BlazorTemplate.Shared.Server.Manages
{
    public class AppSignInManager : SignInManager<UserModel>
    {
        public AppSignInManager(UserManager<UserModel> userManager, IHttpContextAccessor contextAccessor, IUserClaimsPrincipalFactory<UserModel> claimsFactory, IOptions<IdentityOptions> optionsAccessor, ILogger<SignInManager<UserModel>> logger, IAuthenticationSchemeProvider schemes, IUserConfirmation<UserModel> confirmation) : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
        {
        }
    }
}
