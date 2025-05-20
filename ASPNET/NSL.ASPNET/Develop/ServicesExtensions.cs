using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System;
using NSL.ASPNET.Develop.Services;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace NSL.ASPNET.Develop
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddLaunchUrlMonitor(this IServiceCollection services, IWebHostEnvironment env, bool skipExceptions = true)
        {
            var baseDir = AppContext.BaseDirectory;
            var projectDir = Path.GetFullPath(Path.Combine(baseDir, "../../../../../"));

            return services.AddLaunchUrlMonitor(env, Path.Combine(projectDir, "Properties", "launchSettings.json"), skipExceptions);
        }

        public static IServiceCollection AddLaunchUrlMonitor(this IServiceCollection services, IWebHostEnvironment env, string launchSettingsPath, bool skipExceptions = true)
        {
            if (!env.IsDevelopment())
                throw new InvalidOperationException($"{nameof(AddLaunchUrlMonitor)} is intended for development environment only.");

            services.AddSingleton(x => new LaunchUrlReceiveService(x.GetRequiredService<ILogger<LaunchUrlReceiveService>>(), launchSettingsPath, skipExceptions));
            services.AddHostedService(x => x.GetRequiredService<LaunchUrlReceiveService>());

            return services;
        }

        public static IEndpointRouteBuilder MapLaunchUrlMonitor(this IEndpointRouteBuilder builder, string route = "/api/develop/setLaunchUrl")
        {
            builder.MapPost(route, async ([FromBody] string url, [FromServices] LaunchUrlReceiveService service, CancellationToken cancellationToken) =>
            {
                await service.SetLaunchUrl(url, cancellationToken);
            });

            return builder;
        }
    }
}
