using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using NSL.SocketCore.Utils.Buffer;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NSL.SocketPhantom.AspNetCore.Network
{
    public abstract class BasePhantomHub : Hub, IHubContext
    {
        public bool RequiredAuth { get; }

        public PhantomHubOptions Options { get; }

        internal ConcurrentDictionary<string, PhantomHubClientProxy> ClientList { get; } = new();

        internal ConcurrentDictionary<string, List<PhantomHubClientProxy>> UserList { get; } = new();

        IHubClients IHubContext.Clients => base.Clients as IHubClients;

        public BasePhantomHub(bool requiredAuth, PhantomHubOptions options)
        {
            RequiredAuth = requiredAuth;
            Options = options;
            Groups = new PhantomGroupManager(this);

            Clients = new PhantomHubCallerClients();
        }

        public abstract string GetSession(HttpContext context, string? session);

        public abstract string GetSession(ClaimsPrincipal claims, string? session);

        public abstract bool LoadClient(PhantomHubClientProxy client);

        public abstract Task<bool> Invoke(PhantomHubClientProxy client, InputPacketBuffer packet);

        internal abstract void DisconnectClient(PhantomHubClientProxy client);
    }
}
