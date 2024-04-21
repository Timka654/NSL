using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DevExtensions.Authentification.JWT
{
    public class JWTIdentityDataModel
    {
        public string Issuer { get; set; }

        public string Audience { get; set; }

        /// <summary>
        /// Exp lifetime for token in minutes
        /// </summary>
        public long Expires { get; set; }

        public string SecurityKey { get; set; }

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
