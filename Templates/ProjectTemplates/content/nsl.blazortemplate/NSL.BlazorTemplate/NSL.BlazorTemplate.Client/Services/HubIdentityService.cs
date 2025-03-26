using Blazored.LocalStorage;
using NSL.ASPNET.Identity.ClientIdentity;
using NSL.ASPNET.Identity.ClientIdentity.Providers;
using NSL.ASPNET.Identity.JWT;
using NSL.Generators.HttpEndPointGenerator.Shared.Attributes;
using NSL.BlazorTemplate.Shared.Controllers;

namespace NSL.BlazorTemplate.Client.Services
{
    [HttpEndPointImplementGenerate(typeof(IIdentityController))]
    public partial class HubIdentityService(IdentityStateProvider identityStateProvider
        , IHttpClientFactory httpClientFactory
        , ILocalStorageService localStorage) : IdentityJWTService(identityStateProvider)
    {
        protected partial System.Net.Http.HttpClient CreateEndPointClient(string url) 
            => httpClientFactory.CreateClient("ServerAPI");

        private const string StorageKey = "Identity.Token";

        protected override async Task<string?> ReadTokenAsync(CancellationToken cancellationToken = default)
        {
            if (!await localStorage.ContainKeyAsync(StorageKey, cancellationToken))
                return default;

            return await localStorage.GetItemAsStringAsync(StorageKey, cancellationToken);
        }

        protected override async Task SaveTokenAsync(string? token, CancellationToken cancellationToken = default)
        {
            if (token == default)
            {
                await localStorage.RemoveItemAsync(StorageKey, cancellationToken);
                return;
            }

            await localStorage.SetItemAsStringAsync(StorageKey, token, cancellationToken);
        }
    }
}
