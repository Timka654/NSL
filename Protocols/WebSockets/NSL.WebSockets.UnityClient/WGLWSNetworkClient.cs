using NSL.SocketClient;
using NSL.SocketCore.Utils;
using NSL.WebSockets.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.WebSockets.UnityClient
{
    public class WGLWSNetworkClient<T, TOptions> : WSNetworkClient<T, TOptions>
        where T : BaseSocketNetworkClient, new()
        where TOptions : WSClientOptions<T>
    {
        public WGLWSNetworkClient(TOptions options) : base(options)
        {
        }


        protected override WebSocket CreateWS()
        {
            return new WGLWebSocket();
        }

        protected override async Task ConnectAsync(Uri endPoint, CancellationToken cts)
        {
            await ((WGLWebSocket)client).ConnectAsync(endPoint, cts);
        }
    }
}
