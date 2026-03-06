using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace NSL.ASPNET.Routing.ErrorHandler
{
    public static class NSLErrorsHandleMiddlewareExtensions
    {
        public static IServiceCollection AddNSLErrorsHandler(this IServiceCollection services)
            => services.AddNSLErrorsHandler<NSLErrorsHandleService>();

        public static IServiceCollection AddNSLErrorsHandler<THandleService>(this IServiceCollection services) 
            where THandleService : NSLErrorsHandleService
        {
            services.AddSingleton<NSLErrorsHandleService, THandleService>();
            services.AddSingleton<NSLErrorsHandleMiddleware>();

            return services;
        }

        /// <summary>
        /// Use this first after UseCors
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseNSLErrorsHandleMiddleware(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<NSLErrorsHandleMiddleware>();

            return builder;
        }
    }
}
