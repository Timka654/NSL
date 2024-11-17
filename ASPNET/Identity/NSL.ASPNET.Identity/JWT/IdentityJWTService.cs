using NSL.ASPNET.Identity.ClientIdentity;
using NSL.ASPNET.Identity.ClientIdentity.Providers;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Xml.Linq;

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

                UpdateIdentity(value);
            }
        }

        protected virtual void UpdateIdentity(JwtSecurityToken identity)
        {
            _Email = base.Email;
            _UserName = base.UserName;
            _Name = base.Name;
            _Id = base.Id;
            _PhoneNumber = base.PhoneNumber;
            _Roles = base.Roles;
        }

        private string _Email;
        private string _UserName;
        private string _Name;
        private string _Id;
        private string _PhoneNumber;
        private IEnumerable<string> _Roles;

        public override string Email => _Email;

        public override string UserName => _UserName;

        public override string Name => _Name;

        public override string Id => _Id;

        public override string PhoneNumber => _PhoneNumber;

        public override IEnumerable<string> Roles => _Roles;

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
