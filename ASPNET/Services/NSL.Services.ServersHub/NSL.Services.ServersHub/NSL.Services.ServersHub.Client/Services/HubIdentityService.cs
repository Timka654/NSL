using Blazored.LocalStorage;
using NSL.ASPNET.Identity.ClientIdentity;
using NSL.ASPNET.Identity.ClientIdentity.Providers;
using NSL.ASPNET.Identity.JWT;
using NSL.Generators.HttpEndPointGenerator.Shared.Attributes;
using NSL.Services.ServersHub.Shared.Controllers;

namespace NSL.Services.ServersHub.Client.Services
{
    [HttpEndPointImplementGenerate(typeof(IIdentityController))]
    public partial class HubIdentityService(IdentityStateProvider identityStateProvider
        , IHttpClientFactory httpClientFactory
        , ILocalStorageService localStorage) : IdentityJWTService(identityStateProvider)
    {
        protected partial System.Net.Http.HttpClient CreateEndPointClient(string url) 
            => httpClientFactory.CreateClient("ServerAPI");

        private const string StorageKey = "Identity.Token";

        protected override async Task<string?> ReadToken()
        {
            if (!await localStorage.ContainKeyAsync(StorageKey))
                return default;

            return await localStorage.GetItemAsStringAsync(StorageKey);
        }

        protected override async Task SaveToken(string? token)
        {
            if (token == default)
            {
                await localStorage.RemoveItemAsync(StorageKey);
                return;
            }

            await localStorage.SetItemAsync(StorageKey, token);
        }
    }
}
