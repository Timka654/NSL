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
