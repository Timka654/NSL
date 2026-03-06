using System;
using System.Threading.Tasks;

namespace NSL.ASPNET.Localization.Shared
{
    public interface ILocalizationSource
    {
        ushort Order { get; }

        string? GetValue(string key);

        Task ChangeLanguageAsync(string value);

        Task InitializeAsync(ILocalizationService localizationService, IServiceProvider serviceProvider);
    }
}
