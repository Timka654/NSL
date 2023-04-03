using NSL.SocketServer;
using NSL.SocketServer.Utils;
using NSL.UDP.Info;
using NSL.UDP.Interface;
using STUN;
using System.Collections.Generic;
using System.Net;

namespace NSL.UDP
{
    public class UDPClientOptions<TClient> : ServerOptions<TClient>, IBindingUDPOptions, IUDPOptions
        where TClient : IServerNetworkClient
    {
        public string BindingIP { get; set; }

        public int BindingPort { get; set; }

        /// <summary>
        /// Receive messages cycles on initialize
        /// default: 3
        /// </summary>
        public int ReceiveChannelCount { get; set; } = 3;

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
        /// Max send per second bytes rate
        /// default: 1 MBps
        /// </summary>
        public int ClientLimitSendRate { get; set; } = 1 * 1024 * 1024; // 1MB

        /// <summary>
        /// Try repeat send in reliable channel delay
        /// default: 30ms
        /// </summary>
        public int ReliableSendRepeatDelay { get; set; } = 30;

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
