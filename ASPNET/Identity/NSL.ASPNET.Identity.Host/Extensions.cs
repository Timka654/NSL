using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace NSL.ASPNET.Identity.Host
{
    public static class Extensions
    {
        public const string BaseConfigurationPath = "Identity:JWT";

        /// <summary>
        /// Invoke <see cref="AddAPIBaseIdentity"/> before add authentication
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configureAuthentication"></param>
        /// <returns></returns>
        public static AuthenticationBuilder AddDefaultAuthenticationForAPIBaseJWT(
            this IServiceCollection services,
            Action<AuthenticationOptions> configureAuthentication = null)
        {
            return services.AddAuthentication(c =>
            {
                c.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

                if (configureAuthentication != null)
                    configureAuthentication(c);
            });
        }

        public static AuthenticationBuilder AddAPIBaseJWTBearer(this AuthenticationBuilder builder, IConfiguration configuration, Action<JwtBearerOptions> configureBearer = null, string path = BaseConfigurationPath)
            => builder.AddAPIBaseJWTBearer(configuration.GetAPIBaseJWTData(path), configureBearer);

        public static AuthenticationBuilder AddAPIBaseJWTBearer(this AuthenticationBuilder builder, JWTIdentityDataModel data, Action<JwtBearerOptions> configureBearer = null)
            => builder.AddJwtBearer(c =>
                {
                    c.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidIssuer = data.Issuer,
                        ValidAudience = data.Audience,
                        IssuerSigningKey = data.GetSymSecurityKey()
                    };

                    if (configureBearer != null)
                        configureBearer.Invoke(c);
                });

        public static IdentityBuilder AddAPIBaseIdentity<TUser, TRole>(
            this IServiceCollection services,
            Action<IdentityOptions> configureIdentity = null)
            where TRole : class
            where TUser : class
        {
            return services.AddIdentity<TUser, TRole>(c =>
             {
                 c.Password = new PasswordOptions()
                 {
                     RequireDigit = false,
                     RequiredUniqueChars = 0,
                     RequireLowercase = false,
                     RequireUppercase = false,
                     RequireNonAlphanumeric = false
                 };

                 c.Lockout = new LockoutOptions()
                 {
                     AllowedForNewUsers = false
                 };

                 c.SignIn = new SignInOptions()
                 {
                     RequireConfirmedAccount = false,
                     RequireConfirmedEmail = false,
                     RequireConfirmedPhoneNumber = false
                 };

                 if (configureIdentity != null)
                     configureIdentity(c);
             });
        }

        /// <summary>
        /// Register IOptions - <see cref="JWTIdentityDataModel"/>
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IServiceCollection ConfigureAPIBaseJWTData(this IServiceCollection services, IConfiguration configuration, string path = BaseConfigurationPath)
            => services.Configure<JWTIdentityDataModel>(configuration.GetSection(path));

        public static JWTIdentityDataModel GetAPIBaseJWTData(this IConfiguration configuration, string path = BaseConfigurationPath)
            => configuration.GetSection(path)?.Get<JWTIdentityDataModel>();

        public static string GenerateJWT<TId>(
            this IdentityUser<TId> user,
            IConfiguration configuration,
            string path = BaseConfigurationPath)
             where TId : IEquatable<TId>
            => user.GenerateClaims().GenerateJWT(configuration.GetAPIBaseJWTData(path));

        public static string GenerateJWT<TId>(
            this IdentityUser<TId> user,
            JWTIdentityDataModel identityData)
             where TId : IEquatable<TId>
            => user.GenerateClaims().GenerateJWT(identityData);

        public static ClaimsIdentity GenerateClaims<TId>(this IdentityUser<TId> identity, Action<IdentityUser<TId>, List<Claim>> build = null)
             where TId : IEquatable<TId>
        {
            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, identity.Id.ToString()),
                };

            if (identity.UserName != default)
                claims.Add(new Claim(ClaimTypes.Name, identity.UserName));

            if (identity.Email != default)
                claims.Add(new Claim(ClaimTypes.Email, identity.Email));

            if (build != null)
                build(identity, claims);

            return new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);
        }

        public static string GenerateJWT(
            this ClaimsIdentity claims,
            IConfiguration configuration,
            string path = BaseConfigurationPath)
            => claims.GenerateJWT(configuration.GetAPIBaseJWTData(path));

        public static string GenerateJWT(
            this ClaimsIdentity claims,
            JWTIdentityDataModel identityData)
            => identityData.GenerateClaimsToken(claims);

        public static void UseAuth(this IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseAuthorization();
        }

        public static string GetId(this ClaimsPrincipal identity)
            => identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

        public static TResult GetId<TResult>(this ClaimsPrincipal identity)
            where TResult : IConvertible
            => (TResult)Convert.ChangeType(identity.GetId(), typeof(TResult));

        public static async Task<IdentityResult> CreateAccountAsync<TSignInManager, TUser, TKey>(this IHost host, Func<(TUser user, string password)> user)
            where TKey : IEquatable<TKey>
            where TUser : IdentityUser<TKey>
            where TSignInManager : SignInManager<TUser>
        {
            IdentityResult result = default;

            await host.Services.InvokeInScopeAsync(async s =>
            {
                result = await s.CreateAccountAsync<TSignInManager, TUser, TKey>(user);
            });

            return result;
        }

        public static async Task<IdentityResult> CreateAccountAsync<TSignInManager, TUser, TKey>(this IServiceProvider services, Func<(TUser user, string password)> user)
            where TKey : IEquatable<TKey>
            where TUser : IdentityUser<TKey>
            where TSignInManager : SignInManager<TUser>
        {
            var signInManager = services.GetRequiredService<TSignInManager>();

            var u = user();

            if (signInManager.UserManager.Users.Any(x => x.UserName == u.user.UserName))
                return IdentityResult.Success;

            return await signInManager.UserManager.CreateAsync(u.user, u.password);

        }

        public static async Task<IdentityResult> AssignAccountRolesAsync<TSignInManager, TUser, TKey>(this IHost host, TUser user, params string[] roles)
            where TKey : IEquatable<TKey>
            where TUser : IdentityUser<TKey>
            where TSignInManager : SignInManager<TUser>
        {
            IdentityResult result = default;

            await host.Services.InvokeInScopeAsync(async s =>
            {
                result = await s.AssignAccountRolesAsync<TSignInManager, TUser, TKey>(user, roles);
            });

            return result;
        }

        public static async Task<IdentityResult> AssignAccountRolesAsync<TSignInManager, TUser, TKey>(this IServiceProvider services, TUser user, params string[] roles)
            where TKey : IEquatable<TKey>
            where TUser : IdentityUser<TKey>
            where TSignInManager : SignInManager<TUser>
        {
            var signInManager = services.GetRequiredService<TSignInManager>();

            IdentityResult result = IdentityResult.Success;

            foreach (var role in roles)
            {
                if (await signInManager.UserManager.IsInRoleAsync(user, role))
                    continue;

                result = await signInManager.UserManager.AddToRolesAsync(user, roles);

                if (!result.Succeeded)
                    return result;

            }

            return result;
        }

        public static async Task<IdentityResult> CreateRolesAsync<TRoleManager, TRole, TKey>(this IHost host, params string[] roles)
            where TKey : IEquatable<TKey>
            where TRole : IdentityRole<TKey>, new()
            where TRoleManager : RoleManager<TRole>
        {
            IdentityResult result = default;

            await host.Services.InvokeInScopeAsync(async s =>
            {
                result = await s.CreateRolesAsync<TRoleManager, TRole, TKey>(roles);
            });

            return result;
        }

        public static async Task<IdentityResult> CreateRolesAsync<TRoleManager, TRole, TKey>(this IServiceProvider services, params string[] roles)
            where TKey : IEquatable<TKey>
            where TRole : IdentityRole<TKey>, new()
            where TRoleManager : RoleManager<TRole>
        {
            var roleManager = services.GetRequiredService<TRoleManager>();

            IdentityResult result = IdentityResult.Success;

            foreach (var role in roles)
            {

                if (await roleManager.RoleExistsAsync(role))
                    continue;

                result = await roleManager.CreateAsync(new TRole() { Name = role });

                if (!result.Succeeded)
                    return result;
            }

            return result;
        }

        public static string JoinErrors(this IdentityResult result, string splitter = "\r\n")
            => string.Join(splitter, result.Errors.Select(x => x.Description));

        public static void ThrowOnFailed(this IdentityResult result, Func<IdentityResult, Exception> errorBuilder)
        {
            if (result.Succeeded)
                return;

            throw errorBuilder(result);
        }
    }
}
