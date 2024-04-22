using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using NSL.ASPNET.Identity.ClientIdentity.Providers.Options;
using System;
using System.Threading.Tasks;

namespace NSL.ASPNET.Identity.ClientIdentity.Providers
{
    public class IdentityPolicyProvider : IAuthorizationPolicyProvider
    {
        private readonly IdentityPolicyOptions optionsValue;

        public IdentityPolicyProvider(IServiceProvider provider)
        {
            optionsValue = (IdentityPolicyOptions)provider.GetService(typeof(IdentityPolicyOptions));
        }

        private AuthorizationPolicy def = new AuthorizationPolicyBuilder()
            .AddRequirements(new OperationAuthorizationRequirement())
            .Build();

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
            => Task.FromResult(optionsValue?.defaultPolicy ?? def);

        public async Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
            => optionsValue?.fallbackPolicy ?? await GetDefaultPolicyAsync();

        public async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            if (optionsValue?.policyMap.TryGetValue(policyName, out var policy) != true)
                throw new Exception($"Policy {policyName} not be registered");

            return policy ?? await GetDefaultPolicyAsync();
        }
    }
}
