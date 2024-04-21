using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

namespace NSL.ASPNET.MemoryLogger
{
    public static class StartupExtensions
    {
        public static ILoggingBuilder AddMemoryLogger(
            this ILoggingBuilder builder)
        => builder.AddMemoryLogger(MemoryLoggerProvider.DefaultValidTime, MemoryLoggerProvider.DefaultMaxLogCount);

        public static ILoggingBuilder AddMemoryLogger(
            this ILoggingBuilder builder, TimeSpan validTime)
        => builder.AddMemoryLogger(validTime, MemoryLoggerProvider.DefaultMaxLogCount);

        public static ILoggingBuilder AddMemoryLogger(
            this ILoggingBuilder builder, int maxLogCount)
        => builder.AddMemoryLogger(MemoryLoggerProvider.DefaultValidTime, maxLogCount);

        public static ILoggingBuilder AddMemoryLogger(
            this ILoggingBuilder builder, TimeSpan validTime, int maxLogCount)
        {
            builder.AddConfiguration();

            builder.Services.TryAddEnumerable(
                ServiceDescriptor.Singleton<ILoggerProvider, MemoryLoggerProvider>((s) => new MemoryLoggerProvider(validTime, maxLogCount)));

            return builder;
        }

        public static IEndpointConventionBuilder MapDefaultMemoryLoggerViewRoute<T>(this T builder, string pattern)
            where T : IEndpointRouteBuilder
        {
            return builder.MapGet(pattern, c =>
            {
                var lp = c.RequestServices.GetRequiredService<ILoggerProvider>();

                var logs = (lp as MemoryLoggerProvider).GetTextLogs();

                c.Response.ContentType = "text/html";
                c.Response.WriteAsync(string.Join("<br>", logs));

                return Task.CompletedTask;
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <param name="pattern"> can have {categoryName:string}, {logLevel:LogLevel} in pattern</param>
        /// <returns></returns>
        public static IEndpointConventionBuilder MapMemoryLoggerViewRouteWithFilter<T>(this T builder, string pattern)
            where T : IEndpointRouteBuilder
        {
            return builder.MapGet(pattern, (HttpContext c, string? categoryName, LogLevel? logLevel) =>
            {
                var lp = c.RequestServices.GetRequiredService<ILoggerProvider>();

                var logs = (lp as MemoryLoggerProvider).GetLogs();

                if (!string.IsNullOrWhiteSpace(categoryName))
                    logs = logs.Where(x => x.CategoryName.Equals(categoryName, StringComparison.OrdinalIgnoreCase));

                if (logLevel.HasValue)
                    logs = logs.Where(x => x.LogLevel.Equals(logLevel.Value));

                c.Response.ContentType = "text/html";
                c.Response.WriteAsync(string.Join("<br>", logs.Select(x => x.ToString())));

                return Task.CompletedTask;
            });
        }

        /// <summary>
        /// Add default routes with prefix and key in format (<paramref name="prefix"/>)/(<paramref name="key"/>?)
        /// <para>routes:</para>
        /// <para><paramref name="prefix"/>/(<paramref name="key"/>?)</para>
        /// <para><paramref name="prefix"/>/(<paramref name="key"/>?)/byCategory/{categoryName:string}</para>
        /// <para><paramref name="prefix"/>/(<paramref name="key"/>?)/byLogLevel/{logLevel:LogLevel}</para>
        /// <para><paramref name="prefix"/>/(<paramref name="key"/>?)/filter/{categoryName:string}/{logLevel:LogLevel}</para>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <param name="key"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public static T MapMemoryLoggerViewDefaultRoute<T>(this T builder, Guid? key, string prefix = "memory_log")
            where T : IEndpointRouteBuilder
        {
            string prefs = string.Join("/", prefix, key).Trim('/');

            MapDefaultMemoryLoggerViewRoute(builder, $"/{prefs}");
            MapMemoryLoggerViewRouteWithFilter(builder, $"/{prefs}/byCategory/{{categoryName}}");
            MapMemoryLoggerViewRouteWithFilter(builder, $"/{prefs}/byLogLevel/{{logLevel}}");
            MapMemoryLoggerViewRouteWithFilter(builder, $"/{prefs}/filter/{{categoryName}}/{{logLevel}}");

            return builder;
        }

    }
}