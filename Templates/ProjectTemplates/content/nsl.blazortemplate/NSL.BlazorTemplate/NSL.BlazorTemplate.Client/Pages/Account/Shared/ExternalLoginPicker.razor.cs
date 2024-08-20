using Microsoft.AspNetCore.Components;
using NSL.BlazorTemplate.Shared.Client.DotNetIdentity;

namespace NSL.BlazorTemplate.Client.Pages.Account.Shared
{
    public partial class ExternalLoginPicker
    {
        private AuthenticationScheme[] externalLogins = [];

        [SupplyParameterFromQuery] public string? ReturnUrl { get; set; }

        //protected override async Task OnInitializedAsync()
        //{
        //    externalLogins = (await SignInManager.GetExternalAuthenticationSchemesAsync()).ToArray();
        //}
    }
}