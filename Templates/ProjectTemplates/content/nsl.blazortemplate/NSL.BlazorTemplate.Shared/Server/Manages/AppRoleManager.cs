using Microsoft.AspNetCore.Identity;

namespace NSL.BlazorTemplate.Shared.Server.Manages
{
    public class AppRoleManager : RoleManager<IdentityRole<Guid>>
    {
        public AppRoleManager(IRoleStore<IdentityRole<Guid>> store, IEnumerable<IRoleValidator<IdentityRole<Guid>>> roleValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, ILogger<RoleManager<IdentityRole<Guid>>> logger) : base(store, roleValidators, keyNormalizer, errors, logger)
        {
        }
    }
}
