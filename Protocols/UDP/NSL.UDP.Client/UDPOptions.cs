using NSL.SocketClient;
using NSL.SocketCore;
using NSL.SocketServer;
using NSL.SocketServer.Utils;
using NSL.UDP.Client.Info;
using NSL.UDP.Client.Interface;
using NSL.UDP.Interface;
using STUN;
using System.Collections.Generic;
using System.Net;

namespace NSL.UDP.Client
{
    public class UDPClientOptions<TClient> : ClientOptions<TClient>, IBindingUDPOptions, IUDPOptions
        where TClient : BaseSocketNetworkClient
    {
        public string BindingIP { get; set; }

        public int BindingPort { get; set; }

        public List<StunServerInfo> StunServers { get; } = new List<StunServerInfo>();

        public STUNQueryType StunQueryType { get; set; } = STUNQueryType.ExactNAT;

        public IPAddress GetBindingIPAddress() => IPAddress.Parse(BindingIP);

        public IPEndPoint GetBindingIPEndPoint() => new IPEndPoint(GetBindingIPAddress(), BindingPort);

        /// <summary>
        /// 
        /// default: 1024
        /// </summary>
        public int SendFragmentSize { get; set; } = 1024;
    }

    public class UDPServerOptions<TClient> : ServerOptions<TClient>, IBindingUDPOptions, IUDPOptions
        where TClient : IServerNetworkClient
    {
        public string BindingIP { get; set; }

        public int BindingPort { get; set; }

        public List<StunServerInfo> StunServers { get; } = new List<StunServerInfo>();

        public STUNQueryType StunQueryType { get; set; } = STUNQueryType.ExactNAT;

        public IPAddress GetBindingIPAddress() => IPAddress.Parse(BindingIP);

        public IPEndPoint GetBindingIPEndPoint() => new IPEndPoint(GetBindingIPAddress(), BindingPort);

        /// <summary>
        /// 
        /// default: 1024
        /// </summary>
        public int SendFragmentSize { get; set; } = 1024;
    }
}
