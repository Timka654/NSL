using NSL.ASPNET.Localization.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;

public abstract class BaseEditableLocalizationSource : BaseLocalizationSource, IEditableLocalizationSource
{
    public abstract string Name { get; }
    public abstract string NameKey { get; }

    public abstract bool HaveLocalizationPermission();

    public abstract string GetStorageKey(string lang);


    public abstract Task<IEnumerable<BaseStaticLocalizationItemModel>> GetLocalizationValuesAsync(string key);

    public abstract Task<bool> UpdateLocalizationItemAsync(BaseCreateLocalizationItemRequestModel data);
}
