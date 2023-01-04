using NSL.SocketClient;
using NSL.SocketServer;
using NSL.SocketServer.Utils;
using NSL.UDP.Client.Info;
using NSL.UDP.Client.Interface;
using STUN;
using System.Collections.Generic;
using System.Net;

namespace NSL.UDP.Client
{
    public class UDPClientOptions<TClient> : ClientOptions<TClient>, IBindingUDPOptions
        where TClient : BaseSocketNetworkClient
    {
        public string BindingIP { get; set; }

        public int BindingPort { get; set; }

        public List<StunServerInfo> StunServers { get; } = new List<StunServerInfo>();

        public STUNQueryType StunQueryType { get; set; } = STUNQueryType.ExactNAT;

        public IPAddress GetBindingIPAddress() => IPAddress.Parse(BindingIP);

        public IPEndPoint GetBindingIPEndPoint() => new IPEndPoint(GetBindingIPAddress(), BindingPort);
    }

    public class UDPServerOptions<TClient> : ServerOptions<TClient>, IBindingUDPOptions
        where TClient : IServerNetworkClient
    {
        public string BindingIP { get; set; }

        public int BindingPort { get; set; }

        public List<StunServerInfo> StunServers { get; } = new List<StunServerInfo>();

        public STUNQueryType StunQueryType { get; set; } = STUNQueryType.ExactNAT;

        public IPAddress GetBindingIPAddress() => IPAddress.Parse(BindingIP);

        public IPEndPoint GetBindingIPEndPoint() => new IPEndPoint(GetBindingIPAddress(), BindingPort);
    }
}
