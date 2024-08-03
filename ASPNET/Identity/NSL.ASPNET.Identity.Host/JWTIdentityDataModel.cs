using Microsoft.IdentityModel.Tokens;
using NSL.Utils.JsonSchemeGen.Attributes;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NSL.ASPNET.Identity.Host
{
    [NSLJsonScheme("JWT", Path = "Identity")]
    public class JWTIdentityDataModel
    {
        [NSLJsonSchemeProperty(Name = "Issuer")]
        public string Issuer { get; set; }

        [NSLJsonSchemeProperty()]
        public string Audience { get; set; }

        /// <summary>
        /// Exp lifetime for token in minutes
        /// </summary>
        [NSLJsonSchemeProperty()]
        public long Expires { get; set; }

        [NSLJsonSchemeProperty()]
        public string SecurityKey { get; set; }

        [NSLJsonSchemeProperty()]
        public string SecurityAlgorithm { get; set; } = SecurityAlgorithms.HmacSha256;


        public SymmetricSecurityKey GetSymSecurityKey()
            => new SymmetricSecurityKey(
                      Encoding.ASCII.GetBytes(SecurityKey));

        public SigningCredentials GetSignCredentials()
            => new SigningCredentials(
                  GetSymSecurityKey(), SecurityAlgorithm);

        public TimeSpan GetExpiresTimeSpan()
            => TimeSpan.FromMinutes(Expires);

        public string GenerateClaimsToken(ClaimsIdentity claims)
        {
            var now = DateTime.UtcNow;

            var jwt = new JwtSecurityToken(
              issuer: Issuer,
              audience: Audience,
              notBefore: now,
              claims: claims.Claims,
              expires: now.Add(GetExpiresTimeSpan()),
              signingCredentials: GetSignCredentials());

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
    }
}
