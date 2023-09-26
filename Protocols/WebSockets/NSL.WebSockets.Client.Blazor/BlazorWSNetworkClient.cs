using NSL.SocketClient;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace NSL.WebSockets.Client.Blazor
{
    public class BlazorWSNetworkClient<T, TOptions> : WSNetworkClient<T, TOptions>
        where T : BaseSocketNetworkClient, new()
        where TOptions : WSClientOptions<T>
    {
        public BlazorWSNetworkClient(TOptions options) : base(options)
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
