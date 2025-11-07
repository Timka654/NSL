using NSL.ASPNET.Identity.ClientIdentity;
using NSL.ASPNET.Identity.ClientIdentity.Providers;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.ASPNET.Identity.JWT
{
    public abstract class IdentityJWTService : IdentityService
    {
        private JwtSecurityToken identity;

        public IdentityJWTService(IdentityStateProvider identityStateProvider)
            : base(identityStateProvider)
        {
        }

        protected virtual void ReadingClaimsErrorHandle(string jwtToken, Exception ex) { }

        public JwtSecurityToken? Identity { get => identity; private set
            {
                if (identity == value)
                    return;

                identity = value;
            }
        }

        protected virtual Task UpdateIdentity(JwtSecurityToken identity)
        {
            _Email = base.Email;
            _UserName = base.UserName;
            _Id = base.Id;
            _PhoneNumber = base.PhoneNumber;
            _Roles = base.Roles;

            return Task.CompletedTask;
        }

        private string _Email;
        private string _UserName;
        private string _Id;
        private string _PhoneNumber;
        private IEnumerable<string> _Roles = Enumerable.Empty<string>();

        public override string Email => _Email;

        public override string UserName => _UserName;

        public override string Id => _Id;

        public override string PhoneNumber => _PhoneNumber;

        public override IEnumerable<string> Roles => _Roles;

        protected override async Task ReadClaimsAsync(CancellationToken cancellationToken = default)
        {
            if (!IsAuthenticated)
            {
                Identity = default;

                await UpdateIdentity(Identity);
                return;
            }

            try
            {
                Identity = new JwtSecurityTokenHandler().ReadJwtToken(AuthorizationToken);
            }
            catch (Exception ex)
            {
                ReadingClaimsErrorHandle(AuthorizationToken, ex);
                await ClearIdentityAsync(cancellationToken);
            }

            await UpdateIdentity(Identity);
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
