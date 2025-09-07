using Microsoft.AspNetCore.Components;
using NSL.BlazorTemplate.Client.Services;
using NSL.BlazorTemplate.Shared.Models;
using NSL.BlazorTemplate.Shared.Models.RequestModels;

namespace NSL.BlazorTemplate.Client.Pages.Account.Pages.Manage
{
    public partial class Index
    {
        private UserModel user = default!;

        private IdentityIndexRequestModel Input { get; set; } = new();

        [Inject] HubIdentityService IdentityService { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            var response = await IdentityService.UserDetailsPostRequest();

            if (!response.IsSuccess)
                return;

            user = response.Data;

            Input.FillFrom(user);
        }

        private async Task OnValidSubmitAsync()
        {
            var response = await IdentityService.UserEditPostRequest(Input);

            if(response.IsSuccess)
                Input.FillTo(user);
        }
    }
}