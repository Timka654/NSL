using NSL.SocketClient;
using NSL.SocketCore.Utils;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace NSL.WebSockets.Client.Blazor
{
    public class BlazorWSNetworkClient<T> : WSNetworkClient<T>
        where T : BaseNetworkConnection, new()
    {
        public BlazorWSNetworkClient(WSClientOptions<T> options) : base(options)
        {

        }


        protected override WebSocket CreateWS()
        {
            return new ClientWebSocket();
        }

        protected override async Task ConnectAsync(Uri endPoint, CancellationToken cts)
        {
            await ((ClientWebSocket)client).ConnectAsync(endPoint, cts);
        }
}
}
