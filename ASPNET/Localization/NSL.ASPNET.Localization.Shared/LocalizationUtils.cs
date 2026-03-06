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
            services.AddSingleton<ILocalizationService, TType>();

            if(typeof(IEditableLocalizationService).IsAssignableFrom(typeof(TType)))
            {
                services.AddSingleton(typeof(IEditableLocalizationService), typeof(TType));
            }

            return services;
        }
    }
}
