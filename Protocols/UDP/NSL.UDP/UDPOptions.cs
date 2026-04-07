using NSL.SocketCore;
using NSL.SocketCore.Utils;
using NSL.UDP.Info;
using NSL.UDP.Interface;
using STUN;
using System.Collections.Generic;
using System.Net;

namespace NSL.UDP
{
    public class UDPClientOptions<TClient> : CoreOptions, IBindingUDPOptions, IUDPOptions
        where TClient : BaseNetworkConnection, new()
    {
        /// <summary>
        /// Receive messages cycles on initialize
        /// default: 3
        /// </summary>
        public int ReceiveChannelCount { get; set; } = 3;

        public List<StunServerInfo> StunServers { get; } = new List<StunServerInfo>();

        public STUNQueryType StunQueryType { get; set; } = STUNQueryType.ExactNAT;

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

    }
}

