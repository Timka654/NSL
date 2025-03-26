using NSL.ASPNET.Identity.ClientIdentity.Providers;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
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

        protected abstract Task<string?> ReadTokenAsync(CancellationToken cancellationToken = default);

        protected abstract Task SaveTokenAsync(string? token, CancellationToken cancellationToken = default);

        public abstract string GetAuthenticationScheme();

        public abstract IEnumerable<Claim>? GetClaims();


        protected abstract Task ReadClaimsAsync(CancellationToken cancellationToken = default);

        public async Task LoadIdentityAsync(CancellationToken cancellationToken = default)
        {
            await SetIdentityAsync(await ReadTokenAsync(cancellationToken), false);
        }

        public bool ExistsAnyRole(params string[] roleNames)
            => roleNames.Any(roleName => Roles.Contains(roleName));

        public async Task SetIdentityAsync(string? jwtToken, bool save = true, CancellationToken cancellationToken = default)
        {
            AuthorizationToken = jwtToken;

            await ReadClaimsAsync(cancellationToken);

            if (save)
                await SaveTokenAsync(AuthorizationToken, cancellationToken);

            identityStateProvider.LoadAuthState(this);
        }

        public async Task ClearIdentityAsync(CancellationToken cancellationToken = default)
        {
            await SetIdentityAsync(default, true, cancellationToken);
        }

        #endregion

        public abstract HttpClient FillHttpClient(HttpClient client);
    }
}