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
    public partial class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveWebAssemblyComponents();

            builder.AddNSLModelStateFilter();

            AddDatabase(builder.Services, builder.Configuration);

            AddIdentity(builder.Services, builder.Configuration);

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
#if DEBUG
            app.UseHttpsRedirection();
#endif

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
