using NSL.ASPNET.Identity.ClientIdentity;
using NSL.ASPNET.Identity.ClientIdentity.Providers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace NSL.ASPNET.Identity.JWT
{
    public abstract class IdentityJWTService : IdentityService
    {
        public IdentityJWTService(IdentityStateProvider identityStateProvider)
            : base(identityStateProvider)
        {
        }

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
            catch (Exception)
            {
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
