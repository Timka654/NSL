using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NSL.ASPNET.Identity.ClientIdentity;

namespace NSL.ASPNET.Identity.ClientIdentity.MessageHandlers
{
    public class HttpMessageIdentityHandler : DelegatingHandler
    {
        private readonly IdentityService identityService;

        public HttpMessageIdentityHandler(IdentityService identityService)
        {
            this.identityService = identityService;
        }

        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode == false && response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                await identityService.SetIdentity(default);

            return response;
        }
    }
}
