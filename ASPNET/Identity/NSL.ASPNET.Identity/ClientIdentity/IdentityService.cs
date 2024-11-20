using NSL.ASPNET.Identity.ClientIdentity.Providers;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NSL.ASPNET.Identity.ClientIdentity
{
    public abstract class IdentityService
    {
        private readonly IdentityStateProvider identityStateProvider;

        public IdentityService(
            IdentityStateProvider identityStateProvider)
        {
            this.identityStateProvider = identityStateProvider;
        }

        #region AuthorizationToken

        public string? AuthorizationToken { get; protected set; }

        public virtual string? Email
            => GetClaims()?.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;

        public virtual string? UserName
            => GetClaims()?.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;

        public virtual string? Id
            => GetClaims()?.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;

        public virtual string PhoneNumber
            => GetClaims()?.FirstOrDefault(x => x.Type == ClaimTypes.MobilePhone)?.Value ?? string.Empty;

        public virtual IEnumerable<string> Roles
            => GetClaims()?.Where(x => x.Type.Equals(ClaimTypes.Role)).Select(x => x.Value) ?? Enumerable.Empty<string>();

        public bool IsAuthenticated
            => AuthorizationToken != default;

        protected abstract Task<string?> ReadToken();

        protected abstract Task SaveToken(string? token);

        public abstract string GetAuthenticationScheme();

        public abstract IEnumerable<Claim>? GetClaims();


        protected abstract Task ReadClaims();

        public async Task LoadIdentity()
        {
            await SetIdentity(await ReadToken(), false);
        }

        public bool ExistsAnyRole(params string[] roleNames)
            => roleNames.Any(roleName => Roles.Contains(roleName));

        public async Task SetIdentity(string? jwtToken, bool save = true)
        {
            AuthorizationToken = jwtToken;

            await ReadClaims();

            if (save)
                await SaveToken(AuthorizationToken);

            identityStateProvider.LoadAuthState(this);
        }

        public async Task ClearIdentity()
        {
            await SetIdentity(default);
        }

        #endregion

        public abstract HttpClient FillHttpClient(HttpClient client);
    }
}