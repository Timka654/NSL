using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NSL.BlazorTemplate.Shared.Models;

namespace NSL.BlazorTemplate.Shared.Server.Manages
{
    public class AppUserManager : UserManager<UserModel>
    {
        public AppUserManager(IUserStore<UserModel> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<UserModel> passwordHasher, IEnumerable<IUserValidator<UserModel>> userValidators, IEnumerable<IPasswordValidator<UserModel>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<UserModel>> logger) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
        }
    }
}
