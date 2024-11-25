using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NSL.ASPNET.Identity.ClientIdentity
{
    public class IdentityAuthorizationService : IAuthorizationService
    {
        public delegate Task<bool> FilterActionInvokeDelegate(ClaimsPrincipal claims, IAuthorizationRequirement requirement);

        public static Dictionary<Type, FilterActionInvokeDelegate> DefaultFilterActions = new();

        static IdentityAuthorizationService()
        {
            DefaultFilterActions.Add(typeof(OperationAuthorizationRequirement), OperationAuthorizationRequirementFilterHandle);
            DefaultFilterActions.Add(typeof(DenyAnonymousAuthorizationRequirement), OperationAuthorizationRequirementFilterHandle);
            DefaultFilterActions.Add(typeof(RolesAuthorizationRequirement), RolesAuthorizationRequirementFilterHandle);
        }

        public IAuthorizationPolicyProvider PolicyProvider { get; }

        public IdentityAuthorizationService(IAuthorizationPolicyProvider policyProvider)
        {
            PolicyProvider = policyProvider;
        }


        protected virtual Dictionary<Type, FilterActionInvokeDelegate> GetRequirement()
            => DefaultFilterActions;

        private static Task<bool> OperationAuthorizationRequirementFilterHandle(ClaimsPrincipal user, IAuthorizationRequirement req)
            => Task.FromResult(user.Identity.IsAuthenticated);

        private static Task<bool> RolesAuthorizationRequirementFilterHandle(ClaimsPrincipal user, IAuthorizationRequirement req)
        {
            var rolesReq = req as RolesAuthorizationRequirement;

            var result = rolesReq.AllowedRoles.Any(x => user.HasClaim(ClaimTypes.Role, x));

            return Task.FromResult(result);
        }

        public async Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, IEnumerable<IAuthorizationRequirement> requirements)
        {
            var reqs = GetRequirement();

            return (await Task.WhenAll(requirements.Select(x =>
            {
                if (!reqs.TryGetValue(x.GetType(), out var action))
                {
#if DEBUG
                    Console.WriteLine($"Cannot find authorize requirement {x}");
#endif
                    return Task.FromResult(false);
                }

                return action(user, x);
            }))).All(x => x) ? AuthorizationResult.Success() : AuthorizationResult.Failed(AuthorizationFailure.ExplicitFail());
        }

        public async Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, string policyName)
            => await AuthorizeAsync(user, resource, (await PolicyProvider.GetPolicyAsync(policyName))?.Requirements);
    }
}
