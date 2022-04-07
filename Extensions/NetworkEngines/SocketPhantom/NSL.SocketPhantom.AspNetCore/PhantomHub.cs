using Microsoft.AspNetCore.SignalR;
using SocketPhantom.AspNetCore.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
