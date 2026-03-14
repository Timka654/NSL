using Microsoft.Extensions.DependencyInjection;

namespace NSL.ASPNET.Localization.Shared
{
    public static class LocalizationUtils
    {
        public static IServiceCollection AddLocalizationSource<TType>(this IServiceCollection services)
            where TType : class, ILocalizationSource
        {
            return services.AddSingleton<ILocalizationSource, TType>();
        }
        public static IServiceCollection AddLocalizationService<TType>(this IServiceCollection services)
            where TType : class, ILocalizationService
        {
            services.AddSingleton<TType>();
            services.AddSingleton<ILocalizationService, TType>(s => s.GetRequiredService<TType>());

            return services;
        }
    }
}
