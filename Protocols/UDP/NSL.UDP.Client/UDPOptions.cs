using NSL.SocketClient;
using NSL.SocketClient.Utils.SystemPackets;
using NSL.SocketCore.Utils.SystemPackets;
using NSL.SocketServer;
using NSL.SocketServer.Utils;
using NSL.SocketServer.Utils.SystemPackets;
using NSL.UDP.Client.Info;
using NSL.UDP.Client.Interface;
using NSL.UDP.Interface;
using STUN;
using System.Collections.Generic;
using System.Net;

namespace NSL.UDP.Client
{
    public class UDPClientOptions<TClient> : ServerOptions<TClient>, IBindingUDPOptions, IUDPOptions
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

        /// <summary>
        /// Connection IpAddress (0.0.0.0 - all)
        /// </summary>
        public override string IpAddress { get; set; } = "0.0.0.0";

        /// <summary>
        /// Порт - используется для инициализации слушателя на определенном порту 1 - 65,535
        /// </summary>
        public override int Port { get; set; }

        protected override void LoadOptions() { }
    }
}
