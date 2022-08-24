using NSL.SocketClient;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace NSL.WebSockets.Client
{
    public class WSClientOptions<TClient> : ClientOptions<TClient>
        where TClient : BaseSocketNetworkClient
    {
        public override AddressFamily AddressFamily
        {
            get => throw new InvalidOperationException($"{nameof(AddressFamily)} not support for {nameof(WSClientOptions<TClient>)}");
            set => throw new InvalidOperationException($"{nameof(AddressFamily)} not need for {nameof(WSClientOptions<TClient>)}");
        }

        public override ProtocolType ProtocolType
        {
            get => throw new InvalidOperationException($"{nameof(ProtocolType)} not support for {nameof(WSClientOptions<TClient>)}");
            set => throw new InvalidOperationException($"{nameof(ProtocolType)} not need for {nameof(WSClientOptions<TClient>)}");
        }

        /// <summary>
        /// EndPoint list must have bindings in format http(/s)://{bindingAddress}:{bindingPort}/
        /// </summary>
        public Uri EndPoint { get; set; }
    }
}
