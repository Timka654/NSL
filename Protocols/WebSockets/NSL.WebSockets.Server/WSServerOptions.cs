using NSL.SocketServer;
using NSL.SocketServer.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NSL.WebSockets.Server
{
    public class WSServerOptions<TClient> : ServerOptions<TClient>
        where TClient : IServerNetworkClient
    {
        public override AddressFamily AddressFamily
        {
            get => throw new InvalidOperationException($"{nameof(AddressFamily)} not support for {nameof(WSServerOptions<TClient>)}");
            set => throw new InvalidOperationException($"{nameof(AddressFamily)} not need for {nameof(WSServerOptions<TClient>)}");
        }

        public override ProtocolType ProtocolType
        {
            get => throw new InvalidOperationException($"{nameof(ProtocolType)} not support for {nameof(WSServerOptions<TClient>)}");
            set => throw new InvalidOperationException($"{nameof(ProtocolType)} not need for {nameof(WSServerOptions<TClient>)}");
        }

        public override string IpAddress
        {
            get => throw new InvalidOperationException($"{nameof(IpAddress)} not support for {nameof(WSServerOptions<TClient>)} use {nameof(EndPoints)}");
            set => throw new InvalidOperationException($"{nameof(IpAddress)} not need for {nameof(WSServerOptions<TClient>)} use {nameof(EndPoints)}");
        }

        public override int Port
        {
            get => throw new InvalidOperationException($"{nameof(Port)} not support for {nameof(WSServerOptions<TClient>)} use {nameof(EndPoints)}");
            set => throw new InvalidOperationException($"{nameof(Port)} not need for {nameof(WSServerOptions<TClient>)} use {nameof(EndPoints)}");
        }

        /// <summary>
        /// EndPoint list must have bindings in format http(/s)://{bindingAddress}:{bindingPort}/
        /// </summary>
        public List<string> EndPoints { get; } = new List<string>();
    }
}
