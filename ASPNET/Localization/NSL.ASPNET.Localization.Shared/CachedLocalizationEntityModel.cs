using System.Collections.Generic;

namespace NSL.ASPNET.Localization.Shared
{
    public class CachedLocalizationEntityModel
    {
        public required string Content { get; set; }
        public required string? DefaultValue { get; set; }
        public required Dictionary<string, object>? Args { get; set; }
    }
    //public virtual async Task InitializeAsync()
    //{
    //    localizationSources = ServiceProvider
    //        .GetServices<ILocalizationSource>()
    //        .OrderBy(x => x.Order)
    //        .ToArray();

    //    try
    //    {
    //        foreach (var s in localizationSources)
    //        {
    //            await s.InitializeAsync(this, ServiceProvider);
    //        }

    //        //var value = await localStorageService.GetItemAsync<string?>(StorageKey);

    //        //if (value != default)
    //        //{
    //        //    await ChangeLanguage(value);

    //        //    return;
    //        //}

    //        //var markupLanguage = webConfigurationService.Config.Markup.Language.DefaultLangCode;

    //        //if (!string.IsNullOrEmpty(markupLanguage))
    //        //{
    //        //    await ChangeLanguage(markupLanguage.Trim().ToLower());

    //        //    return;
    //        //}

    //        await ChangeLanguage(DefaultLanguage);

    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine(ex.ToString());
    //    }
    //}
}
