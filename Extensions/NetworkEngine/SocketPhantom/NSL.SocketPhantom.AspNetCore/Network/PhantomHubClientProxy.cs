using Microsoft.AspNetCore.SignalR;
using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketPhantom.Enums;
using NSL.SocketServer.Utils;
using NSL.WebSockets.Server.AspNetPoint;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.SocketPhantom.AspNetCore.Network
{
    public class PhantomHubClientProxy : AspNetWSNetworkServerClient, IClientProxy
    {
        public PhantomHubCallerContext Context { get; private set; }

        public string UserId => Claims?.FindFirstValue(ClaimsIdentity.DefaultNameClaimType) ?? default;

        public ClaimsPrincipal Claims { get; internal set; }

        public BasePhantomHub Hub { get; internal set; }

        public PhantomHubClientProxy() : base()
        {
            Context = new PhantomHubCallerContext();
            Context.Initialize(this);
        }

        public Task SendCoreAsync(string method, object[] args, CancellationToken cancellationToken = default)
        {
            var p = OutputPacketBuffer.Create(PacketEnum.Invoke);

            p.WriteString16(method);

            p.WriteCollection(args, (p, i) => { p.WriteJson16(i); });

            if (Network?.GetState() == true)
                Network.Send(p);

            return Task.CompletedTask;
        }

        public async Task SendCoreAsync(string method, params object[] args)
        {
            await SendCoreAsync(method, args, CancellationToken.None);
        }

        public override void ChangeOwner(IServerNetworkClient from)
        {
            base.ChangeOwner(from);

            var c = from as PhantomHubClientProxy;

            Session = c.Session;
            Network = c.Network;

            if (!LastReceiveMessage.HasValue)
                LastReceiveMessage = c.LastReceiveMessage;

            Hub = c.Hub;
        }
    }
}
