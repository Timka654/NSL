using SocketServer;
using SocketServer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NSL.UDP.Client
{
    public class UDPOptions<TClient> : ServerOptions<TClient>
        where TClient : IServerNetworkClient
    {
        public string BindingIP { get; set; }

        public int BindingPort { get; set; }

        public IPAddress GetBindingIPAddress() => IPAddress.Parse(BindingIP);

        public IPEndPoint GetBindingIPEndPoint() => new IPEndPoint(GetBindingIPAddress(), BindingPort);
    }
}
