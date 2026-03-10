using Microsoft.Extensions.DependencyInjection;
using NSL.ASPNET.Localization.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IEditableLocalizationSource : ILocalizationSource
{
    bool HaveLocalizationPermission();

    string Name { get; }

    string NameKey { get; }

    bool PresetAllowed { get; }

    Task<bool> UpdateLocalizationItemAsync(string currentLanguage, string key, string value);

    Task<IEnumerable<BaseStaticLocalizationItemModel>> GetLocalizationValuesAsync(string key);
}

public static class LocalizationUtils
{
    public static IServiceCollection AddEditableLocalizationService<TType>(this IServiceCollection services)
        where TType : class, IEditableLocalizationService
    {
        services.AddSingleton(typeof(IEditableLocalizationService), s => s.GetRequiredService<TType>());

        return services;
    }
}
