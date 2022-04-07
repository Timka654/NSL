using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.SocketPhantom.AspNetCore
{
    public class PhantomTempClientProxy : IClientProxy
    {
        private readonly IEnumerable<IClientProxy> clients;

        public PhantomTempClientProxy(IEnumerable<IClientProxy> clients)
        {
            this.clients = clients;
        }

        public async Task SendCoreAsync(string method, object[] args, CancellationToken cancellationToken = default)
        {
            await Task.WhenAll(clients.Select(x => x.SendCoreAsync(method, args, cancellationToken)));
        }
    }
}
