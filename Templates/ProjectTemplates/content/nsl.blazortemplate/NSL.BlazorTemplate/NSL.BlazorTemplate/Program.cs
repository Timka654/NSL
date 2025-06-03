using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NSL.ASPNET.Identity.Host;
using NSL.Database.EntityFramework.ASPNET;
using NSL.BlazorTemplate.Client.Pages;
using NSL.BlazorTemplate.Shared.Models;
using NSL.BlazorTemplate.Shared.Server.Data;
using NSL.BlazorTemplate.Shared.Server.Manages;
using System.Net;
using NSL.ASPNET.Mvc.Route;

namespace NSL.BlazorTemplate
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveWebAssemblyComponents();

            builder.AddNSLModelStateFilter();

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();
            

            builder.Services.AddAPIBaseIdentity<UserModel, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddSignInManager<AppSignInManager>()
                .AddUserManager<AppUserManager>()
                .AddRoleManager<AppRoleManager>();

            builder.Services.AddDefaultAuthenticationForAPIBaseJWT()
                .AddAPIBaseJWTBearer(builder.Configuration);

            builder.Services.ConfigureApplicationCookie(c =>
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


            var app = builder.Build();

            await app.AcceptDbMigrations<ApplicationDbContext>();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseAuth();

            app.UseStaticFiles();
            //app.UseAntiforgery();

            app.MapDefaultControllerRoute();

            app.UseBlazorFrameworkFiles();
            app.MapFallbackToFile("index.html");

            // Add additional endpoints required by the Identity /Account Razor components.
            //app.MapAdditionalIdentityEndpoints();

            app.Run();
        }
    }
}
