using Microsoft.IdentityModel.Tokens;
using NSL.Utils.JsonSchemeGen.Attributes;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace NSL.ASPNET.Identity.Host
{
    [NSLJsonScheme("JWT", Path = "Identity")]
    public class JWTIdentityDataModel
    {
        [NSLJsonSchemeProperty(Name = "Issuer")]
        public string Issuer { get; set; } = "default";

        [NSLJsonSchemeProperty()]
        public string Audience { get; set; } = "default";

        /// <summary>
        /// Exp lifetime for token in minutes
        /// </summary>
        [NSLJsonSchemeProperty()]
        public long Expires { get; set; } = 3600;

        [NSLJsonSchemeProperty()]
        public string SecurityKey { get; set; }

        [NSLJsonSchemeProperty()]
        public string SecurityAlgorithm { get; set; } = SecurityAlgorithms.HmacSha256;


        SymmetricSecurityKey? key;
        SigningCredentials? credentials;

        public SymmetricSecurityKey GetSymSecurityKey()
            => key ??= GenerateOrLoadKey();

        private SymmetricSecurityKey GenerateOrLoadKey()
        {
            if (!string.IsNullOrWhiteSpace(SecurityKey))
            {
                return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecurityKey));
            }

            byte[] randomKeyBytes = RandomNumberGenerator.GetBytes(32);
            return new SymmetricSecurityKey(randomKeyBytes);
        }

        public SigningCredentials GetSignCredentials()
            => credentials ??= new SigningCredentials(
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
