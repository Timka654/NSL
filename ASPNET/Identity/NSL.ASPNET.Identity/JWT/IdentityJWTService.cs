using NSL.ASPNET.Identity.ClientIdentity;
using NSL.ASPNET.Identity.ClientIdentity.Providers;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NSL.ASPNET.Identity.JWT
{
    public abstract class IdentityJWTService : IdentityService
    {
        public IdentityJWTService(IdentityStateProvider identityStateProvider)
            : base(identityStateProvider)
        {
        }

        protected virtual void ReadingClaimsErrorHandle(string jwtToken, Exception ex) { }

        public JwtSecurityToken? Identity { get; private set; }

        protected override async Task ReadClaims()
        {
            if (!IsAuthenticated)
            {
                Identity = default;
                return;
            }

            try
            {
                Identity = new JwtSecurityTokenHandler().ReadJwtToken(AuthorizationToken);
            }
            catch (Exception ex)
            {
                ReadingClaimsErrorHandle(AuthorizationToken, ex);
                await ClearIdentity();
            }
        }

        public override IEnumerable<Claim>? GetClaims()
            => Identity?.Claims;

        public override string GetAuthenticationScheme()
            => "Bearer";

        public override HttpClient FillHttpClient(HttpClient client)
        {
            if (IsAuthenticated)
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(GetAuthenticationScheme(), AuthorizationToken);

            return client;
        }
    }
}
