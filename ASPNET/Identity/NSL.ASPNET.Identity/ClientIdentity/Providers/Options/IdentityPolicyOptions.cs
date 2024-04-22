using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;

namespace NSL.ASPNET.Identity.ClientIdentity.Providers.Options
{
    public class IdentityPolicyOptions
    {
        internal Dictionary<string, AuthorizationPolicy> policyMap = new();

        internal AuthorizationPolicy fallbackPolicy;
        internal AuthorizationPolicy defaultPolicy;

        public IdentityPolicyOptions RegisterPolicy(string name, IEnumerable<IAuthorizationRequirement> requirements)
            => RegisterPolicy(name, requirements, default);

        public IdentityPolicyOptions RegisterPolicy(string name, IEnumerable<IAuthorizationRequirement> requirements, IEnumerable<string> authenticationSchemes)
            => RegisterPolicy(name, new AuthorizationPolicy(requirements, authenticationSchemes));

        public IdentityPolicyOptions RegisterPolicy(string name, Action<AuthorizationPolicyBuilder> buildAction)
        {
            var builder = new AuthorizationPolicyBuilder();
            buildAction(builder);

            return RegisterPolicy(name, builder.Build());
        }

        public IdentityPolicyOptions RegisterPolicy(string name, AuthorizationPolicy policy)
        {
            if (!policyMap.TryAdd(name, policy))
                throw new Exception($"policy key {name} already registred");

            return this;
        }

        public IdentityPolicyOptions RegisterFallbackPolicy(AuthorizationPolicy policy)
        {
            fallbackPolicy = policy;

            return this;
        }

        public IdentityPolicyOptions RegisterDefaultPolicy(AuthorizationPolicy policy)
        {
            defaultPolicy = policy;

            return this;
        }

        public static IdentityPolicyOptions Create() => new IdentityPolicyOptions();
    }
}
