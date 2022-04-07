using Microsoft.AspNetCore.SignalR;
using SocketPhantom.AspNetCore.Network;

namespace SocketPhantom.AspNetCore
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
