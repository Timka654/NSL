using Microsoft.AspNetCore.Identity;
using NSL.ASPNET.Identity.Host;
using NSL.BlazorTemplate.Shared.Models;
using NSL.BlazorTemplate.Shared.Server.Data;
using NSL.BlazorTemplate.Shared.Server.Manages;
using System.Net;

namespace NSL.BlazorTemplate
{
    public partial class Program
    {
        static void AddIdentity(IServiceCollection services, IConfiguration configuration)
        {
            services.AddAPIBaseIdentity<UserModel, IdentityRole<Guid>>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddSignInManager<AppSignInManager>()
                .AddUserManager<AppUserManager>()
                .AddRoleManager<AppRoleManager>();

            services.AddDefaultAuthenticationForAPIBaseJWT()
                .AddAPIBaseJWTBearer(configuration);

            services.ConfigureApplicationCookie(c =>
            {
                c.Events = new Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationEvents()
                {
                    OnRedirectToAccessDenied = c =>
                    {
                        c.Response.StatusCode = (int)HttpStatusCode.Forbidden;

                        return Task.CompletedTask;
                    },
                    OnRedirectToLogin = c =>
                    {
                        c.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

                        return Task.CompletedTask;
                    }
                };
            });
        }
    }
}
