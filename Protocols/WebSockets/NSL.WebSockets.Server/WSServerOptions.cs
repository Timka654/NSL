using NSL.SocketCore.Utils;
using NSL.SocketServer;
using NSL.SocketServer.Utils;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace NSL.WebSockets.Server
{
    public class WSServerOptions<TClient> : ServerOptions<TClient>
        where TClient : BaseNetworkConnection, new()
    {
        public override AddressFamily AddressFamily
        {
            get => throw new InvalidOperationException($"Property {nameof(AddressFamily)} not support for {nameof(WSServerOptions<TClient>)}");
            set => throw new InvalidOperationException($"Property {nameof(AddressFamily)} not need for {nameof(WSServerOptions<TClient>)}");
        }

        public override ProtocolType ProtocolType
        {
            get => throw new InvalidOperationException($"Property {nameof(ProtocolType)} not support for {nameof(WSServerOptions<TClient>)}");
            set => throw new InvalidOperationException($"Property  {nameof(ProtocolType)} not need for {nameof(WSServerOptions<TClient>)}");
        }

        /// <summary>
        /// EndPoint list must have bindings in format http(/s)://{bindingAddress}:{bindingPort}/
        /// WARNING!!! "0.0.0.0" unsupported, you can use "*" or "+"
        /// </summary>
        public List<string> EndPoints { get; } = new List<string>();
    }
}
