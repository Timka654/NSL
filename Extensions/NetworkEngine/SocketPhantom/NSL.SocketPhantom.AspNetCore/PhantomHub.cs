using Microsoft.AspNetCore.SignalR;
using NSL.SocketPhantom.AspNetCore.Network;

namespace NSL.SocketPhantom.AspNetCore
{
    public class PhantomHub : Hub
    {
        internal void SetBase(BasePhantomHub baseHub, PhantomHubClientProxy client)
        {
            Clients = ((PhantomHubCallerClients)baseHub.Clients).Clone(client);
            Groups = baseHub.Groups;
            Context = client.Context;
        }
    }
}
