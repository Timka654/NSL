﻿using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NSL.ASPNET.Identity.ClientIdentity.Providers
{
    public class IdentityStateProvider : AuthenticationStateProvider
    {
        private AuthenticationState _anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        private AuthenticationState? currentState;

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
            => Task.FromResult(currentState ?? _anonymous);

        public void LoadAuthState(IdentityService identityService)
        {
            if (identityService.IsAuthenticated)
                currentState = new AuthenticationState(
                    new ClaimsPrincipal(
                        new ClaimsIdentity(
                            identityService.GetClaims(),
                            identityService.GetAuthenticationScheme()
                            )));
            else
                currentState = null;

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
