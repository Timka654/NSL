using NSL.ASPNET.Localization.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public abstract class BaseLocalizationSource : ILocalizationSource
{
    public abstract ushort Order { get; }

    protected abstract bool IsActive();

    protected Dictionary<string, string> CurrentLibrary = new Dictionary<string, string>();

    public string? GetValue(string key)
    {
        CurrentLibrary.TryGetValue(key, out var value);

        return value;
    }

    public abstract Task ChangeLanguageAsync(string lang);

    protected ILocalizationService localizationService;

    public virtual Task InitializeAsync(ILocalizationService localizationService, IServiceProvider serviceProvider)
    {
        this.localizationService = localizationService;

        return Task.CompletedTask;
    }
}
