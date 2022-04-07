using Microsoft.AspNetCore.SignalR;
using NSL.SocketPhantom.AspNetCore.Network;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.SocketPhantom.AspNetCore
{
    public class PhantomOthersClientProxy : IClientProxy
    {
        private readonly PhantomHubCallerClients callerClients;

        public PhantomOthersClientProxy(PhantomHubCallerClients callerClients)
        {
            this.callerClients = callerClients;
        }

        public async Task SendCoreAsync(string method, object[] args, CancellationToken cancellationToken = default)
        {
            await callerClients.AllExcept(((PhantomHubClientProxy)callerClients.Caller).Session).SendCoreAsync(method, args, cancellationToken);
        }
    }
}
